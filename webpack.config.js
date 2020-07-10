
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    entry: {
        index: './Scripts/out/main.js'
    },
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, './Assets/Examples/Scripts/')
    },
    mode: "development", 
    plugins: [
        new CleanWebpackPlugin()
    ]
}