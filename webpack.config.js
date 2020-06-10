
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    entry: {
        index: './src/js/index.js'
    },
    output: {
        filename: '[name].js',
        path: path.resolve(__dirname, '../out')
    },
    mode: "development", 
    plugins: [
        new CleanWebpackPlugin()
    ]
}