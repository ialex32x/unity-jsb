
const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const fs = require('fs');

const jsbm = JSON.parse(fs.readFileSync('jsb-modules.json'));
let external_modules = {};
for (let m of jsbm.modules) {
    external_modules[m] = `commonjs2 ${m}`;
}
external_modules['jsb'] = 'commonjs2 jsb';

module.exports = {
    entry: {
        example_main: './Scripts/src/example_main.ts'
    },
    output: {
        // filename: 'main.js',
        filename: '[name].js',
        path: path.resolve(__dirname, './Scripts/dist')
    },
    externals: external_modules,
    module: {
        rules: [{
            test: /\.tsx?$/,
            parser: { amd: false, system: false },
            use: 'ts-loader',
            // use: 'awesome-typescript-loader', 
            exclude: /node_modules/
        }]
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js']
    },
    devtool: 'inline-source-map',
    devServer: {
        contentBase: './Scripts/dist',
        port: 8183,
        hot: true
    },
    mode: 'development',
    plugins: [
        new CleanWebpackPlugin(),
        new CopyWebpackPlugin({
            patterns: [
                {
                    from: path.resolve(__dirname, './Scripts/dist'),
                    to: path.resolve(__dirname, './Assets/Examples/Resources/dist'),
                    transformPath: (targetPath, absolutePath) => {
                        return targetPath + ".txt";
                    },
                    toType: "dir"
                },
                {
                    from: path.resolve(__dirname, './Scripts/protogen'),
                    to: path.resolve(__dirname, './Assets/Examples/Resources/protogen'),
                    transformPath: (targetPath, absolutePath) => {
                        return targetPath + ".txt";
                    },
                    toType: "dir"
                },
                {
                    from: path.resolve(__dirname, './Scripts/config'),
                    to: path.resolve(__dirname, './Assets/Examples/Resources/config'),
                    transformPath: (targetPath, absolutePath) => {
                        return targetPath + ".txt";
                    },
                    toType: "dir"
                }
            ]
        }),
        // new webpack.HotModuleReplacementPlugin()
        new HtmlWebpackPlugin({ template: './Scripts/src/index.html' })
    ]
}
