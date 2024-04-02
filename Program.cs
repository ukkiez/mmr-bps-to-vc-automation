using System;
using System.IO;
using System.Linq;
namespace ModifyROM {
  class Program {
    static readonly int FILESIZE_32MB = 0x02000000;
    public static string ROMFILEPATH = string.Empty;
    public static string CURRENTDIR = string.Empty;
    enum ExitCode : int {
      Success = 0,
      Error = 1,
    }

    static byte[] getFile50(int romsize) {
      BinaryReader file50Reader = new BinaryReader(File.OpenRead(Path.Combine(CURRENTDIR, "5-0")));
      byte[] buffer = new byte[file50Reader.BaseStream.Length];
      file50Reader.Read(buffer, 0, buffer.Length);

      // if over 32MB, rom size and file offsets in the included "5-0" u8
      // archive chunk need to be updated
      if (romsize > FILESIZE_32MB) {
        buffer[0x574] = (byte)(romsize >> 24 & 0xFF);
        buffer[0x575] = (byte)(romsize >> 16 & 0xFF);
        buffer[0x576] = (byte)(romsize >> 8 & 0xFF);
        buffer[0x577] = (byte)((romsize & 0xFF) + 4);

        int entryoffset = 0x0578;
        for (int i = 0; i < 0x4C; i++) {
          if ((buffer[entryoffset] & 0x01) == 0x00) {
            int fileoffset = (
              (buffer[entryoffset + 4] << 24)
              + (buffer[entryoffset + 5] << 16)
              + (buffer[entryoffset + 6] << 8)
              + buffer[entryoffset + 7]
              + (romsize - FILESIZE_32MB)
            );

            buffer[entryoffset + 4] = (byte)(fileoffset >> 24 & 0xFF);
            buffer[entryoffset + 5] = (byte)(fileoffset >> 16 & 0xFF);
            buffer[entryoffset + 6] = (byte)(fileoffset >> 8 & 0xFF);
            buffer[entryoffset + 7] = (byte)(fileoffset & 0xFF);
          }

          entryoffset += 0xC;
        }
      }

      file50Reader.Close();
      return buffer.ToArray();
    }

    static byte[] addVCHeader(byte[] ROM) {
      // this header dictates how much space in wii memory needs to be allocated
      // for the ROM
      byte[] Header;
      if (ROM.Length > FILESIZE_32MB) {
        int headersize = ROM.Length << 2;
        Header = new byte[] {
          (byte)(headersize >> 24 & 0xFF),
          (byte)(headersize >> 16 & 0xFF),
          (byte)(headersize >> 8 & 0xFF),
          (byte)(headersize & 0xFF)
        };
      }
      else{
        Header = new byte[] { 0x08, 0x00, 0x00, 0x00 };
      }

      return Header.Concat(ROM).ToArray();
    }

    static int Main(string[] args) {
      if (args.Length > 0) {
        ROMFILEPATH = args[0];
      }

      if (ROMFILEPATH == string.Empty) {
        return (int)ExitCode.Error;
      }

      CURRENTDIR = System.IO.Directory.GetCurrentDirectory();

      BinaryReader ROMReader = new BinaryReader(File.OpenRead(ROMFILEPATH));
      byte[] ROM = new byte[ROMReader.BaseStream.Length];
      ROMReader.Read(ROM, 0, ROM.Length);
      ROMReader.Close();

      ROM = addVCHeader(ROM);

      BinaryWriter app5 = new BinaryWriter(File.Open(Path.Combine(CURRENTDIR, "00000005.app"), FileMode.Create));

      byte[] file50Buffer = getFile50(ROM.Length - 4);
      app5.Write( file50Buffer );
      app5.Write( ROM );

      BinaryReader file51Reader = new BinaryReader(File.OpenRead(Path.Combine(CURRENTDIR, "5-1")));
      byte[] a51Buffer = new byte[file51Reader.BaseStream.Length];
      file51Reader.Read(a51Buffer, 0, a51Buffer.Length);
      app5.Write( a51Buffer );

      file51Reader.Close();
      app5.Close();

      return (int)ExitCode.Success;
    }
  }
}
