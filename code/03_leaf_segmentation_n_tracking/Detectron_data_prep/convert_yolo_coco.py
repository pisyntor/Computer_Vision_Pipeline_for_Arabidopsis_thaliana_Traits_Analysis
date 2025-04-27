import os
import yaml
import json
from PIL import Image

def get_all_files_in_nested_folder(root_folder, extensions):
    """
    Recursively find all files with specified extensions in a nested folder structure.
    """
    files = []
    for root, _, filenames in os.walk(root_folder):
        for filename in filenames:
            if filename.lower().endswith(extensions):
                files.append(os.path.join(root, filename))
    return files

def parse_yaml(yaml_path):
    """
    Parse the YAML file to extract dataset information.
    """
    with open(yaml_path, "r") as f:
        data = yaml.safe_load(f)
    dataset_root = data.get("path", ".")
    train_images = os.path.join(dataset_root, data["train"])
    val_images = os.path.join(dataset_root, data["val"])
    classes = data["names"]
    return train_images, val_images, classes

def yolo_to_coco(yaml_path, labels_root, output_path):
    # Parse YAML file
    train_images, val_images, classes = parse_yaml(yaml_path)

    # Initialize COCO structure
    coco = {
        "images": [],
        "annotations": [],
        "categories": []
    }
    annotation_id = 1
    image_id = 1

    # Add categories from YAML file
    for class_id, class_name in classes.items():
        coco["categories"].append({
            "id": int(class_id) + 1,  # COCO categories are 1-indexed
            "name": class_name,
            "supercategory": "none"
        })

    # Process train and val image directories
    for image_dir in [train_images, val_images]:
        image_files = get_all_files_in_nested_folder(image_dir, (".jpg", ".png"))

        for image_file in image_files:
            # Get image metadata
            with Image.open(image_file) as img:
                width, height = img.size

            # Generate unique image ID and filename
            image_filename = os.path.relpath(image_file, train_images)
            coco["images"].append({
                "id": image_id,
                "file_name": image_filename.replace("\\", "/"),  # Normalize path for JSON
                "width": width,
                "height": height
            })

            # Locate corresponding label file
            relative_path = os.path.splitext(os.path.relpath(image_file, train_images))[0]
            label_file = os.path.join(labels_root, f"{relative_path}.txt")

            if os.path.exists(label_file):
                with open(label_file, "r") as f:
                    for line in f:
                        data = line.strip().split()
                        class_id = int(data[0])
                        center_x, center_y, bbox_width, bbox_height = map(float, data[1:5])
                        polygon = list(map(float, data[5:]))

                        # Convert YOLO bbox to COCO bbox
                        x_min = (center_x - bbox_width / 2) * width
                        y_min = (center_y - bbox_height / 2) * height
                        bbox_width = bbox_width * width
                        bbox_height = bbox_height * height
                        bbox = [x_min, y_min, bbox_width, bbox_height]

                        # Convert YOLO polygon (normalized) to COCO absolute coordinates
                        abs_polygon = [
                            coord * (width if i % 2 == 0 else height)
                            for i, coord in enumerate(polygon)
                        ]

                        # Add annotation
                        coco["annotations"].append({
                            "id": annotation_id,
                            "image_id": image_id,
                            "category_id": class_id + 1,  # COCO is 1-indexed
                            "segmentation": [abs_polygon],  # COCO expects a list of polygons
                            "bbox": bbox,
                            "iscrowd": 0,
                            "area": bbox_width * bbox_height
                        })
                        annotation_id += 1

            image_id += 1

    # Save COCO JSON
    with open(output_path, "w") as f:
        json.dump(coco, f, indent=4)

# Example usage
yaml_path = ""
labels_root = ""
output_path = ""

yolo_to_coco(yaml_path, labels_root, output_path)
