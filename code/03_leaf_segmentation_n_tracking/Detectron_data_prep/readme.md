## 🧾 What is COCO Format (for Detectron2)?

COCO (Common Objects in Context) is a widely used dataset format for object detection and segmentation tasks. **Detectron2** expects datasets in COCO format when using `register_coco_instances()`.

### The COCO JSON file has three main sections:

1. **`images`** – Contains metadata about each image:
   - `id`: a unique ID for the image
   - `file_name`: image filename
   - `width` and `height`: image dimensions

2. **`annotations`** – Contains info for each object instance:
   - `id`: a unique ID for the annotation
   - `image_id`: links to the image where the object appears
   - `category_id`: class label of the object
   - `bbox`: bounding box in the format `[x_min, y_min, width, height]`
   - `area`: area of the object (used for evaluation)
   - `iscrowd`: used for distinguishing single objects from crowds
   - `segmentation`: optional polygon info for instance segmentation

3. **`categories`** – Maps class IDs to class names:
   - Each entry includes `id` and `name` (e.g., id: 0, name: "person")

---

## 🔁 Converting a YOLO Dataset to COCO Format

### How YOLO Format Works:
- Each image has an associated `.txt` file.
- Each line in the file represents an object:
  - `<class_id> <x_center> <y_center> <width> <height>`
- All values are **normalized** between 0 and 1.
- Bounding boxes are defined by their **center point** and size.

### Steps to Convert:
1. **Read each YOLO annotation file** and its corresponding image.
2. **Convert bounding box values** from normalized `[x_center, y_center, width, height]` to absolute COCO format `[x_min, y_min, width, height]`.
3. **Collect image info** like filename, width, and height.
4. **Map class IDs to category names** to build the `categories` section.
5. **Generate a COCO-style JSON file** with the converted info.

---

## ✅ Why Convert to COCO?
- Detectron2 doesn’t support YOLO format directly.
---
 