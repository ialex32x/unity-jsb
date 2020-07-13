
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = {
    entry: {
        index: './Assets/Examples/Scripts/src/main.ts'
    },
    output: {
        filename: 'main.js',
        path: path.resolve(__dirname, './Assets/Examples/Scripts/dist')
    },
    module: {
        rules: [{
            test: /\.tsx?$/, 
            parser: { amd: false, system: false } ,
            use: 'ts-loader', 
            // use: 'awesome-typescript-loader', 
            exclude: /node_modules/
        }]
    }, 
    resolve: {
        extensions: ['.tsx', '.ts', '.js']
    },
    mode: 'development', 
    plugins: [
        new CleanWebpackPlugin()
    ]
}
