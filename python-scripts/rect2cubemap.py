import cv2
import numpy as np

# Define input image path
input_image_path = "input_image.jpg"

# Load input image
input_image = cv2.imread(input_image_path)

# Define output image size
output_size = (512, 512)

# Define cubic map
cubemap = np.zeros((output_size[0], output_size[1] * 6, 3), dtype=np.uint8)

# Define equirectangular map
equirectangular_map_x = np.zeros(cubemap.shape[:2], dtype=np.float32)
equirectangular_map_y = np.zeros(cubemap.shape[:2], dtype=np.float32)

# Define cubemap face order
# Define cubemap face order
face_order = [0, 1, 3, 2, 5, 4]  # Order: +X, -X, +Y, -Y, +Z, -Z

# Define projection matrix
focal_length = 0.5 * output_size[1] / np.tan(np.pi / 6)
projection_matrix = np.array([[focal_length, 0, output_size[1] / 2],
                              [0, focal_length, output_size[0] / 2],
                              [0, 0, 1]])

# Compute equirectangular map
for y in range(cubemap.shape[0]):
    for x in range(cubemap.shape[1]):
        cube_x = (2 * (x + 0.5) / cubemap.shape[1] - 1) * np.pi / 2
        cube_y = (1 - 2 * (y + 0.5) / cubemap.shape[0]) * np.pi

        equirectangular_x = np.cos(cube_y) * np.sin(cube_x)
        equirectangular_y = np.sin(cube_y)
        equirectangular_z = np.cos(cube_y) * np.cos(cube_x)

        equirectangular_vector = np.array([equirectangular_x, equirectangular_y, equirectangular_z])
        equirectangular_vector = equirectangular_vector / np.linalg.norm(equirectangular_vector)

        theta = np.arctan2(equirectangular_vector[1], equirectangular_vector[0])
        phi = np.arccos(equirectangular_vector[2])

        equirectangular_map_x[y, x] = theta / (2 * np.pi) * input_image.shape[1]
        equirectangular_map_y[y, x] = phi / np.pi * input_image.shape[0]

# Convert equirectangular map to cubemap faces
for i, face in enumerate(face_order):
    x_start = i * output_size[1]
    x_end = (i + 1) * output_size[1]

    cubemap_face = cv2.remap(input_image, equirectangular_map_x, equirectangular_map_y, face, projection_matrix)
    cubemap[:, x_start:x_end] = cubemap_face

# Save output cubemap image
output_image_path = "cubemap_image.jpg"
cv2.imwrite(output_image_path, cubemap)
