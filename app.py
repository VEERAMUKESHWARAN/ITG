import streamlit as st
import numpy as np
import yfinance as yf
from keras.models import load_model
from sklearn.preprocessing import MinMaxScaler
import matplotlib.pyplot as plt

# Function to preprocess stock data
def preprocess_data(stock_data, time_step=60):
    stock_data = stock_data[['Close']]  # Use only 'Close' price
    scaler = MinMaxScaler(feature_range=(0, 1))
    scaled_data = scaler.fit_transform(stock_data)

    def create_dataset(data, time_step=60):
        x, y = [], []
        for i in range(len(data) - time_step - 1):
            x.append(data[i:(i + time_step), 0])
            y.append(data[i + time_step, 0])
        return np.array(x), np.array(y)

    x_data, y_data = create_dataset(scaled_data, time_step)
    x_data = x_data.reshape(x_data.shape[0], x_data.shape[1], 1)
    return x_data, y_data, scaler

# Function to predict next 7 days
def predict_next_7_days(stock_ticker, input_date):
    # Get stock data up to the given date
    stock_data = yf.download(stock_ticker, start="2010-01-01", end=input_date)

    # Preprocess data
    x_data, y_data, scaler = preprocess_data(stock_data)

    # Load the model for the given stock ticker
    model = load_model(f"{stock_ticker}_lstm_model.h5")

    # Get the last 60 days data for prediction
    last_60_days = x_data[-1].reshape(1, -1, 1)
    
    # Predict next 7 days stock prices
    predicted_prices = []
    for _ in range(7):
        pred_price = model.predict(last_60_days)
        predicted_prices.append(pred_price[0][0])
        
        # Update input for the next prediction
        last_60_days = np.append(last_60_days[:, 1:, :], pred_price.reshape(1, 1, 1), axis=1)

    # Inverse transform the predicted prices to the original scale
    predicted_prices = scaler.inverse_transform(np.array(predicted_prices).reshape(-1, 1))

    # Actual stock prices for the last 7 days
    actual_prices = stock_data[-7:]['Close'].values

    return predicted_prices, actual_prices

# Streamlit app
st.markdown(
    """
    <style>
    .title {
        font-size: 2em;
        color: #333;
        text-align: center;
        font-weight: bold;
    }
    .selectbox, .date_input, .button {
        margin-top: 10px;
        padding: 10px;
        font-size: 1em;
    }
    .button {
        background-color: #4CAF50;
        color: white;
        border: none;
        cursor: pointer;
    }
    .button:hover {
        background-color: #45a049;
    }
    .error {
        color: red;
        font-weight: bold;
    }
    </style>
    """, unsafe_allow_html=True)

st.markdown('<div class="title">Stock Price Prediction for the Next 7 Days</div>', unsafe_allow_html=True)
st.write("Select a stock ticker and the end date to predict the next 7 days' prices.")

# Dropdown menu for stock selection
stocks = ['GOOGL', 'TSLA', 'ADBE', 'AMD', 'TEAM', 'CSCO', 'DASH', 'HON', 'MU', 'META', 'AAPL']
stock_ticker = st.selectbox('Select Stock Ticker', stocks, key='selectbox')

# Date input
input_date = st.date_input('Select Date', key='date_input')

# Button to trigger prediction
if st.button('Predict', key='button'):
    try:
        # Predict next 7 days stock prices and get actual prices
        predicted_prices, actual_prices = predict_next_7_days(stock_ticker, input_date)
        
        # Plot the predicted and actual prices
        fig, ax = plt.subplots(figsize=(10, 5))
        ax.plot(range(1, 8), predicted_prices.flatten(), label='Predicted Prices', marker='o')
        ax.plot(range(1, 8), actual_prices, label='Actual Prices', marker='x')
        
        ax.set_title(f'{stock_ticker} - Predicted vs Actual Prices for the Next 7 Days')
        ax.set_xlabel('Days')
        ax.set_ylabel('Price')
        ax.legend()
        
        # Display the plot in Streamlit
        st.pyplot(fig)
    except Exception as e:
        st.markdown(f'<div class="error">Error: {str(e)}</div>', unsafe_allow_html=True)
