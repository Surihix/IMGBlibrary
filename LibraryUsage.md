# Library usage

As we are dealing with DDS type image files, each image file is made up of one or more mips or mip maps. each mip data is stored in the IMGB file and the necessary offsets/position for each mip is given in the image header block file.

This header block file contains all the necessary information about the image such as the DDS pixel format, the image type, the number of mips that make up the image, and the dimensions of the image. all of this information, will be present after the `GTEX` section in the file and for more information on the `GTEX` section, please refer to this [page](https://github.com/LR-Research-Team/Datalog/wiki/TRB#gtex).

## Unpacking
The `IMGBUnpack.UnpackIMGB()` function will unpack each mip data stored in the IMGB file using the image header block file. 

This function requires these following parameters to be specified:
- `imgHeaderBlockFile`
<br>Full path to the image header block file with the `GTEX` section.

- `inImgbFile`
<br>Full path to the `.imgb` file.

- `extractIMGBdir`
<br>Full path to the folder in which you want to unpack the image file.

- `imgbPlatform`
<br>The platform of the IMGB and the header block file. if you are dealing with a PC version file, use the `win32` enum. if its the PS3 or Xbox 360 versions, then use either the `ps3` or `x360` enum.

- `showLog`
<br>Set this to `true`, if you want to see parsing related information that will be used when unpacking the image file. 

### General notes
- If the unpacked image filenames end with `_cbmap_#`, then the image file is part of a cubemap set that uses a single header block file. the pixel format, dimensions, and mip counts are all shared by the images belonging to this set.

- If the unpacked image filenames end with `_stack_#`, then the image file is part of a stack set that uses a single header block file. the pixel format, dimensions, and mip count are all shared by the images belonging to this set. do note that the image files should contain only one mip and if there are multiple mips, then the image file itself will not be unpacked.

- For a list of image types, please refer to this [page](https://github.com/LR-Research-Team/Datalog/wiki/TRB#texture-type).

- For PS3 version image files that are swizzled or in a different color order, this function will first unpack the mip, unswizzles the mip, and then change the color order to BGRA from ARGB.

- As I do not know the unswizzling method for the Xbox 360 version image files, this function will only unpack the mips without unswizzling them.

## Repacking
The `IMGBRepack` class provides two types of repacking functions to repack image files into the IMGB file. repacking is supported only for the PC version image files.

### Repack Function 1
The `IMGBRepack.RepackIMGBType1()` function will repack the image file only when the image file's pixel format, dimensions, mip count are all same as the original image file.<br>This function is recommended for repacking image files that are inside `.xgr` files and the header block file will not be updated by this function.

This function requires these following parameters to be specified:
- `imgHeaderBlockFile`
<br>Full path to the image header block file with the `GTEX` section. the image file should also contain the same name as this header block file, but with the `.dds` extension.

- `outImgbFile`
<br> Full path to the `.imgb` file. the file should be present in the path.

- `extractedIMGBdir`
<br>Full path to the folder in which the unpacked image file is present.

- `imgbPlatform`
<br>The platform of the IMGB and the header block file. use the `win32` enum.

- `showLog`
<br>Set this to `true`, if you want to see parsing related information that will be used when repacking the image file.

### Repack Function 2
The `IMGBRepack.RepackIMGBType2()` function can be used to repack image files that has a different pixel format (see [supported](https://github.com/LR-Research-Team/Datalog/wiki/TRB#texture-format) formats), different dimensions, and different mip count compared to the original image file.

This function is useful for repacking image files that are modified heavily compared to the original file and is recommended for repacking image files that are inside `.trb` and `.imgb` files. the header block file will be updated by this function.

This function requires these following parameters to be specified:
- `tmpImgHeaderBlockFile`
<br>Full path to the image header block file with the `GTEX` section. its better to use a temporary copy of the file to ensure that the original file remains safe, if any exceptions or errors occur inside this function. if you are sure that no errors will occur with the file, then you can use the original file's path itself.

- `imgHeaderBlockFileName`
<br>The name of the image header block file. the image file should also contain the same name as this header block file, but with the `.dds` extension.

- `outImgbFile`
<br>Full path to the `.imgb` file. the file may or may not exist in the path.

- `extractedIMGBdir`
<br>Full path to the folder in which the unpacked image file is present.

- `imgbPlatform`
<br>The platform of the IMGB and the header block file. use the `win32` enum.

- `showLog`
<br>Set this to `true`, if you want to see parsing related information that will be used when repacking the image file.

### General notes
- If you have repacked an image with a different pixel format and the image doesn't look proper ingame, then try using the same pixel format as the orignal image file. this issue can occur when the shader used by the game is expecting the pixel format of the image to be similar to the original image.
- Do not modify image files that are mean't to be used by the game's shaders. you can identify these image files by their content and the non standard dimensions. modfying these images can cause all sorts of rendering issues to crop up ingame. 
