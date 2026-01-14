"""
Create app icon using PIL (no Cairo dependency).
"""
from PIL import Image, ImageDraw
import os

def create_icon(size, output_path):
    """Create a barcode scanner icon at the specified size."""
    
    # Create image with teal gradient background
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Background gradient (teal to dark teal)
    for y in range(size):
        # Gradient from #00838F to #004D54
        r = int(0 + (0 - 0) * y / size)
        g = int(131 - (131 - 77) * y / size)
        b = int(143 - (143 - 84) * y / size)
        draw.line([(0, y), (size, y)], fill=(r, g, b, 255))
    
    # Calculate proportions
    margin = int(size * 0.12)
    bar_area_top = int(size * 0.20)
    bar_area_bottom = int(size * 0.65)
    bar_area_height = bar_area_bottom - bar_area_top
    
    # Draw barcode bars (white)
    bar_positions = [0.15, 0.22, 0.28, 0.35, 0.42, 0.48, 0.55, 0.62, 0.68, 0.75, 0.82]
    bar_widths = [0.03, 0.02, 0.04, 0.02, 0.03, 0.02, 0.04, 0.02, 0.03, 0.02, 0.03]
    
    for pos, width in zip(bar_positions, bar_widths):
        x = int(size * pos)
        w = int(size * width)
        draw.rectangle(
            [x, bar_area_top, x + w, bar_area_bottom],
            fill=(255, 255, 255, 240)
        )
    
    # Scanner frame corners (cyan)
    corner_color = (0, 229, 255, 255)
    corner_length = int(size * 0.12)
    corner_thickness = int(size * 0.02)
    
    frame_left = margin
    frame_right = size - margin
    frame_top = int(size * 0.12)
    frame_bottom = int(size * 0.72)
    
    # Top-left corner
    draw.rectangle([frame_left, frame_top, frame_left + corner_length, frame_top + corner_thickness], fill=corner_color)
    draw.rectangle([frame_left, frame_top, frame_left + corner_thickness, frame_top + corner_length], fill=corner_color)
    
    # Top-right corner
    draw.rectangle([frame_right - corner_length, frame_top, frame_right, frame_top + corner_thickness], fill=corner_color)
    draw.rectangle([frame_right - corner_thickness, frame_top, frame_right, frame_top + corner_length], fill=corner_color)
    
    # Bottom-left corner
    draw.rectangle([frame_left, frame_bottom - corner_thickness, frame_left + corner_length, frame_bottom], fill=corner_color)
    draw.rectangle([frame_left, frame_bottom - corner_length, frame_left + corner_thickness, frame_bottom], fill=corner_color)
    
    # Bottom-right corner
    draw.rectangle([frame_right - corner_length, frame_bottom - corner_thickness, frame_right, frame_bottom], fill=corner_color)
    draw.rectangle([frame_right - corner_thickness, frame_bottom - corner_length, frame_right, frame_bottom], fill=corner_color)
    
    # Scanning line (red)
    scan_line_y = int(size * 0.42)
    scan_line_height = int(size * 0.015)
    draw.rectangle(
        [frame_left, scan_line_y, frame_right, scan_line_y + scan_line_height],
        fill=(255, 82, 82, 255)
    )
    
    # Box icon at bottom left
    box_size = int(size * 0.10)
    box_x = int(size * 0.35)
    box_y = int(size * 0.78)
    draw.rectangle(
        [box_x, box_y, box_x + box_size, box_y + box_size],
        fill=(255, 255, 255, 230),
        outline=(0, 131, 143, 255),
        width=2
    )
    # Box top fold
    draw.polygon(
        [(box_x + 2, box_y + box_size//3), 
         (box_x + box_size//2, box_y + 2), 
         (box_x + box_size - 2, box_y + box_size//3)],
        fill=(0, 131, 143, 200)
    )
    
    # Checkmark circle at bottom right
    check_x = int(size * 0.55)
    check_y = int(size * 0.78)
    check_size = int(size * 0.10)
    draw.ellipse(
        [check_x, check_y, check_x + check_size, check_y + check_size],
        fill=(76, 175, 80, 255)
    )
    # Checkmark
    check_points = [
        (check_x + check_size * 0.25, check_y + check_size * 0.5),
        (check_x + check_size * 0.42, check_y + check_size * 0.68),
        (check_x + check_size * 0.75, check_y + check_size * 0.32)
    ]
    draw.line(check_points[:2], fill=(255, 255, 255, 255), width=max(2, int(size * 0.02)))
    draw.line(check_points[1:], fill=(255, 255, 255, 255), width=max(2, int(size * 0.02)))
    
    # Save
    img.save(output_path, 'PNG')
    print(f"[OK] Created: {output_path} ({size}x{size})")

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Create icons at various sizes
    sizes = [512, 256, 192, 144, 128, 96, 72, 48]
    
    print("Creating app icons...")
    print("-" * 40)
    
    for size in sizes:
        output_path = os.path.join(script_dir, f"icon_{size}x{size}.png")
        create_icon(size, output_path)
    
    print("-" * 40)
    print(f"[OK] All icons created in: {script_dir}")

if __name__ == "__main__":
    main()
