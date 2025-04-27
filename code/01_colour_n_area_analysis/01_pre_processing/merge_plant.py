import os
from PIL import Image

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

    # Open images and get their sizes
    images = [Image.open(os.path.join(folder_path, img)) for img in image_files]
    heights = [img.height for img in images]
    widths = [img.width for img in images]

    # Calculate total width and max height for the resulting image
    total_width = sum(widths)
    max_height = max(heights)

    # Create a new blank image with the calculated dimensions
    stacked_image = Image.new('RGB', (total_width, max_height))

    # Paste each image into the resulting image
    current_x = 0
    for img in images:
        stacked_image.paste(img, (current_x, 0))
        current_x += img.width

    # Save the resulting image as PNG
    stacked_image.save(output_file, format="PNG")
    print(f"Stacked image saved as: {os.path.abspath(output_file)}")


def process_all_subfolders(root_folder):
    """
    Processes all `segmented_images` subfolders within `rep` folders in the root folder,
    stacking images and saving them as `foldername_merged.png`.
    
    Args:
        root_folder (str): Path to the root folder containing subfolders.
    """
    # Get all `rep` folders in the root folder
    rep_folders = [f.path for f in os.scandir(root_folder) if f.is_dir()]
    
    if not rep_folders:
        print("No `rep` folders found in the specified folder.")
        return

    for rep_folder in rep_folders:
        segmented_images_folder = os.path.join(rep_folder, 'segmented_images')
        if os.path.exists(segmented_images_folder):
            folder_name = os.path.basename(rep_folder)
            output_file = os.path.join(root_folder, f"{folder_name}_merged.png")
            print(f"Processing folder: {segmented_images_folder}")
            stack_images_horizontally(segmented_images_folder, output_file)
        else:
            print(f"No `segmented_images` folder found in: {rep_folder}")


# Example usage
if __name__ == "__main__":
    # Replace this with your root folder path
    root_folder_path = "labells/Col-0/"
    process_all_subfolders(root_folder_path)

