### **1. `03_SAM_fine_tuning`**
This notebook focuses on **fine-tuning SAM (Segment Anything Model)** for a segmentation task. It includes:
- Data loading and preparation (`image_paths`, `mask_paths`)
- Data augmentation 
- Use of `segmentation_models_pytorch` and `DiceLoss` for model training

---

### **2. `01_encoder_decoder_models_training`**
This notebook appears to be handling the **training loop and optimizer experimentation** for a segmentation task:
- Different optimizers (`SGD`, `Adam`, `RMSprop`, `AdamW`)
- Loading of data splits from a JSON file
- Use of `segmentation_models_pytorch` for model architecture

---

### **3. `02_EDC_model_mask_generation`**
This notebook focuses on **mask processing and noise filtering** during inference:
- Noise detection and removal using contour analysis with OpenCV
- Dilation and morphological operations
- Likely used to clean predicted segmentation masks before evaluation or use

---

### **4. `04_SAM_mask_generation`**
A notebook for **generating segmentation masks using SAM**:
- Reuses a similar data loader and augmentation setup as `SAM_fine_tuning`
- Uses `transform` logic (e.g., flipping) for image-mask preprocessing
- Includes full pipeline: from loading data to producing and possibly saving masks

---
 