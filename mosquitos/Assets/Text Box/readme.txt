Thank you for using Catlike Coding's Text Box!


CONTACT, FEEDBACK, AND SUPPORT

You can contact Catlike Coding via

http://catlikecoding.com/contact/

You can also post in the appropriate topics on the Unity forums.

DOCUMENTATION

Online documentation for all assets and components, both for editor use and for scripting, is available at

http://catlikecoding.com/unity/documentation/

It can also be found in the menu via

Help / Catlike Coding Documentation


IMPORTING FONTS

Text Box has its own font asset type which contains bitmap font data. The CCFont documentation describes where to get TTF fonts, how to convert them to bitmap fonts, import them into Unity, and generate distance maps for their font atlases.

http://catlikecoding.com/unity/documentation/CCFont/


DISTANCE MAP GENERATOR

If you want to use fancy font shaders, you need to convert font atlases into distance maps. Catlike Coding has a free Distance Map Generator tool that you can download from

http://catlikecoding.com/unity/products/distance-map-generator/


DEMO SCENE

The demo scene contains a single text box and a GUI controller, along with a few gradients and modifiers. It allows you play with a variety text configurations, content, and shaders. Use a large game view or maximize on play so the GUI fits in the window and you get a nice view of the text.

The demo scene includes embedding data for a subset of three fonts:

"Andika" http://scripts.sil.org/andika

"Gentium" http://scripts.sil.org/gentium

"LauHoWi-a" http://www.jpberlin.de/attacwtal-agrar/kultur/lauhowi.html

These fonts are licensed under the SIL Open Font License. For more information, see

http://scripts.sil.org/OFL

To add more fonts to the demo, you have to convert your own TrueType fonts to bitmap format, import them, and generate distance maps for them. Then add them to the GUIController's "Font Packages" array.


GIZMOS

If you want to see specific icons for the components and assets, move everything inside the Text Box / Gizmos folder to a Gizmos folder in the project's asset root.


WORKFLOW EXAMPLE

A typical workflow when you start with a new project goes like this.

- Obtain a TTF version of the font you want. Either a system font or a font downloaded from the internet.
- Use a tool to convert this font into a BMFont-compatible bitmap font, resulting in a FNT and a PNG file.
- Create a new bitmap font via Assets / Create / Text Box / Font.
- Import the FNT file into the font asset.
- Include the font atlas PNG in your project and generate a distance map from it.
- Create a new material, set it to use a text shader and the appropriate texture.
- Create a new text box via GameObject / Create Other / Text Box.
- Set the box's font and material.
- Start typing text and tweaking the box.


HISTORY

1.0.2
Added CCTextCylinderWrapper.
Added MeshCollider support.
Added multi-object editing support.
Improved WYSIWYG editing.

1.0.1
Fixed prefab WYSIWYG editing.
Removed files that didn't belong to the product.

1.0.0
Initial version.
