import os
from PIL import Image

def crop_image(image, crop_percentage=0.12):
    """
    Crops an image by removing a percentage from all sides.
    
    Args:
        image (PIL.Image.Image): The image to crop.
        crop_percentage (float): The percentage of width/height to remove from each side.
    
    Returns:
        PIL.Image.Image: The cropped image.
    """
    width, height = image.size
    crop_width = int(width * crop_percentage)
    crop_height = int(height * crop_percentage)
    cropped_image = image.crop((crop_width, crop_height, width - crop_width, height - crop_height))
    return cropped_image

def stack_images_horizontally(folder_path, output_file):
    """
    Stacks images horizontally from the given folder and saves them as a single PNG image.
    
    Args:
        folder_path (str): Path to the folder containing images.
        output_file (str): Name of the output image file.
    """
    # Check if folder exists
    if not os.path.exists(folder_path):
        print(f"Folder does not exist: {folder_path}")
        return
    
    # Get all image files in the folder, sorted alphabetically
    image_files = sorted([f for f in os.listdir(folder_path) if f.lower().endswith(('png', 'jpg', 'jpeg', 'bmp', 'gif'))])
    
    if not image_files:
        print(f"No image files found in folder: {folder_path}")
        return

    # Open and crop images
    images = [crop_image(Image.open(os.path.join(folder_path, img))) for img in image_files]
    heights = [img.height for img in images]
    widths = [img.width for img in images]

    # Calculate total width and max height for the resulting image
    total_width = sum(widths)
    max_height = max(heights)

    # Create a new blank image with the calculated dimensions
    stacked_image = Image.new('RGB', (total_width, max_height))

    # Paste each cropped image into the resulting image
    current_x = 0
    for img in images:
        stacked_image.paste(img, (current_x, 0))
        current_x += img.width

    # Save the resulting image as PNG
    stacked_image.save(output_file, format="PNG")
    print(f"Stacked image saved as: {os.path.abspath(output_file)}")


def process_all_subfolders(root_folder):
    """
    Processes all subfolders in the root folder, stacking cropped images in each
    subfolder and saving them as `foldername_merged.png`.
    
    Args:
        root_folder (str): Path to the root folder containing subfolders.
    """
    # Get all subfolders in the root folder
    subfolders = [f.path for f in os.scandir(root_folder) if f.is_dir()]
    
    if not subfolders:
        print("No subfolders found in the specified folder.")
        return

    for subfolder in subfolders:
        folder_name = os.path.basename(subfolder)
        output_file = os.path.join(root_folder, f"{folder_name}_merged.png")
        
        print(f"Processing folder: {subfolder}")
        stack_images_horizontally(subfolder, output_file)


# Example usage
if __name__ == "__main__":
    # Replace this with your root folder path
    root_folder_path = "output/Col-0/"
    process_all_subfolders(root_folder_path)
