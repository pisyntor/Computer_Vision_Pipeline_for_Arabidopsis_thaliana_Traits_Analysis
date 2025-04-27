from PIL import Image
import numpy as np
from collections import Counter
from matplotlib import pyplot as plt
from pathlib import Path
from tqdm import tqdm
import os


def RGB2HEX(color):
    return "#{:02x}{:02x}{:02x}".format(int(color[0]), int(color[1]), int(color[2]))


def color_region(image, num_clusters=8):
    """
    Segment the image into color clusters and calculate their proportions, excluding black pixels.
    """
    # Convert image to RGB numpy array
    pixel_values = np.array(image).reshape((-1, 3))

    # Remove black pixels
    pixel_values = pixel_values[~np.all(pixel_values == [0, 0, 0], axis=1)]

    # Perform k-means clustering using sklearn
    from sklearn.cluster import KMeans
    kmeans = KMeans(n_clusters=num_clusters, random_state=42, n_init=10, max_iter=300)
    kmeans.fit(pixel_values)

    # Get cluster centers and labels
    centers = np.uint8(kmeans.cluster_centers_)
    labels = kmeans.labels_

    # Count occurrences of each cluster
    counts = Counter(labels)
    counts = dict(sorted(counts.items()))

    # Map clusters to HEX colors
    ordered_colors = [centers[i] for i in counts.keys()]
    hex_colors = [RGB2HEX(ordered_colors[i]) for i in counts.keys()]
    color_ratios = [count / sum(counts.values()) for count in counts.values()]

    return hex_colors, color_ratios


def generate_pie_chart(image_path, output_path, num_clusters=8):
    """
    Generate a pie chart of color distribution from an image and save it.
    """
    # Open the image using Pillow
    image = Image.open(image_path).convert("RGB")

    # Get HEX colors and their proportions
    hex_colors, color_ratios = color_region(image, num_clusters)

    # Extract image name from the path
    image_name = Path(image_path).stem

    # Plot pie chart
    fig, ax = plt.subplots(figsize=(6, 6))
    ax.pie(color_ratios, labels=hex_colors, colors=hex_colors, autopct='%1.1f%%')
    plt.title(f"Color Distribution: {image_name}")

    # Save the pie chart
    plt.savefig(output_path, bbox_inches="tight")
    plt.close()


def process_images_in_folders(base_folder, num_clusters=8):
    """
    Iterate over all folders and images in the current folder,
    generate pie charts, and save them in the 'output' folder.
    """
    base_path = Path(base_folder)
    output_base = base_path / "output"

    # Gather all image paths
    all_images = [p for p in base_path.rglob("*") if p.is_file() and p.suffix.lower() in [".png", ".jpg", ".jpeg"]]

    print(f"Found {len(all_images)} images to process.")

    # Process each image with progress bar
    for image_path in tqdm(all_images, desc="Processing images"):
        # Define output path
        relative_path = image_path.relative_to(base_path)
        output_path = output_base / relative_path.parent / f"{image_path.stem}_pie_chart.png"

        # Create output directories if they don't exist
        output_path.parent.mkdir(parents=True, exist_ok=True)

        # Generate and save the pie chart
        generate_pie_chart(image_path, output_path, num_clusters)
        print(f"Processed: {image_path} -> Saved pie chart at: {output_path}")


# Example usage:
if __name__ == "__main__":
    current_folder = Path.cwd()
    print(f"Starting processing in folder: {current_folder}")
    process_images_in_folders(current_folder)
    print("Processing completed.")
