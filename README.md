Used to automate the process going from BPS-patched z64 files to WAD files, for [MMR](https://github.com/ZoeyZolotova/mm-rando/).

The batch file calls `modify_bps_patched_z64_for_vc.exe` to inject VC header/functionality, which can be complex to do manually if the ROM is larger than 32MB, and subsequently calls the standard `wadpacker` CLI to generate the final WAD.

## Usage
1. Add both the `.exe` and `.bat` file to your `vc` folder in your MMR installation.
2. Drag a BPS-patched .z64 onto the `.bat` file.
