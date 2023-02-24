from tensorflow import keras

def init_solitaire_model(input_shape, output_shape, num_hidden_layers, hidden_layer_size):
    model = keras.Sequential()
    
    # add the input layer
    model.add(keras.layers.Input(shape=input_shape))
    
    # add hidden layers
    for i in range(num_hidden_layers):
        model.add(keras.layers.Dense(hidden_layer_size, activation='relu'))
    
    # add output layer
    model.add(keras.layers.Dense(output_shape))
    
    return model