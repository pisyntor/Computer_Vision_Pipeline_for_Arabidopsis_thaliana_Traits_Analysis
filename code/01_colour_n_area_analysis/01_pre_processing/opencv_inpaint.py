import os
import cv2
import numpy as np
from tqdm import tqdm
import natsort

# Define paths
raw_data_path = "raw_data"
labels_path = "labells"
output_base_path = "output"

# Create output base path if it doesn't exist
os.makedirs(output_base_path, exist_ok=True)

# Iterate over main categories (Ba4-1, Col-0, etc.)
for category in os.listdir(raw_data_path):
    raw_category_path = os.path.join(raw_data_path, category)
    labels_category_path = os.path.join(labels_path, category)
    output_category_path = os.path.join(output_base_path, category)
    os.makedirs(output_category_path, exist_ok=True)

    # Iterate over replicates (rep_01, rep_02, etc.)
    for replicate in os.listdir(raw_category_path):
        raw_replicate_path = os.path.join(raw_category_path, replicate)
        masks_path = os.path.join(labels_category_path, replicate, "masks")
        output_replicate_path = os.path.join(output_category_path, replicate)
        os.makedirs(output_replicate_path, exist_ok=True)

        # Check if masks path exists
        if not os.path.exists(masks_path):
            print(f"Mask folder missing: {masks_path}")
            continue

        # Process all images in the replicate folder
        all_files = natsort.natsorted(os.listdir(raw_replicate_path))
        for image_id in tqdm(all_files, desc=f"Processing {replicate} in {category}"):
            image_path = os.path.join(raw_replicate_path, image_id)
            mask_name = image_id[:-4] + "_mask.png"
            mask_path = os.path.join(masks_path, mask_name)

            # Load original image and mask
            org_image = cv2.imread(image_path)
            mask_img = cv2.imread(mask_path, cv2.IMREAD_GRAYSCALE)

            if org_image is None or mask_img is None:
                continue

            # Resize mask to match original image dimensions
            mask_img = cv2.resize(mask_img, (org_image.shape[1], org_image.shape[0]))

            # Apply morphological operations
            kernel = np.ones((3, 3), np.uint8)
            mask_img = cv2.dilate(mask_img, kernel, iterations=1)
            mask_img = cv2.erode(mask_img, kernel, iterations=1)

            # Inpaint the image
            back_ground_img = cv2.inpaint(org_image, mask_img, 10, cv2.INPAINT_TELEA)

            # Save the result in the output folder
            output_image_path = os.path.join(output_replicate_path, image_id)
            cv2.imwrite(output_image_path, back_ground_img)