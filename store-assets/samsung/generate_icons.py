"""
Generate app store icons from SVG source.
Run: pip install cairosvg pillow
Then: python generate_icons.py
"""

import os
import sys

try:
    from PIL import Image
    import cairosvg
    from io import BytesIO
except ImportError:
    print("Installing required packages...")
    os.system("pip install cairosvg pillow")
    from PIL import Image
    import cairosvg
    from io import BytesIO

def generate_icon(svg_path, output_path, size):
    """Convert SVG to PNG at specified size"""
    try:
        # Convert SVG to PNG bytes
        png_data = cairosvg.svg2png(
            url=svg_path,
            output_width=size,
            output_height=size
        )
        
        # Open with PIL and save
        img = Image.open(BytesIO(png_data))
        img = img.convert('RGBA')
        img.save(output_path, 'PNG')
        print(f"✓ Generated: {output_path} ({size}x{size})")
        return True
    except Exception as e:
        print(f"✗ Error generating {output_path}: {e}")
        return False

def main():
    # Paths
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(os.path.dirname(script_dir))
    svg_path = os.path.join(project_root, "AcumaticaInventoryScanner", "Resources", "Images", "app_icon.svg")
    
    # Alternative SVG path
    if not os.path.exists(svg_path):
        svg_path = os.path.join(project_root, "AcumaticaInventoryScanner", "Resources", "AppIcon", "appiconfg.svg")
    
    print(f"Source SVG: {svg_path}")
    print(f"Output directory: {script_dir}")
    print()
    
    if not os.path.exists(svg_path):
        print(f"Error: SVG file not found at {svg_path}")
        sys.exit(1)
    
    # Required icon sizes for Samsung Galaxy Store
    sizes = {
        "icon_512x512.png": 512,
        "icon_216x216.png": 216,
        "icon_192x192.png": 192,
        "icon_144x144.png": 144,
        "icon_96x96.png": 96,
        "icon_72x72.png": 72,
        "icon_48x48.png": 48,
    }
    
    print("Generating icons...")
    print("-" * 40)
    
    success_count = 0
    for filename, size in sizes.items():
        output_path = os.path.join(script_dir, filename)
        if generate_icon(svg_path, output_path, size):
            success_count += 1
    
    print("-" * 40)
    print(f"Generated {success_count}/{len(sizes)} icons")
    
    if success_count == 0:
        print("\nNote: If cairosvg fails, you can manually convert the SVG using:")
        print("  - Online converter: https://cloudconvert.com/svg-to-png")
        print("  - Inkscape: File > Export PNG Image")
        print(f"\nSource SVG: {svg_path}")

if __name__ == "__main__":
    main()
