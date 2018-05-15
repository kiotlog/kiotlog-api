const path = require('path');

module.exports = [{
  entry: "./app.js",
  output: {
    path: path.resolve(__dirname, 'js'),
    filename: "kiotlog-bundle.js"
  },
  mode: 'production',
  module: {
    rules: [
      {
        test: /\.js$/,
        use: [
          {
            loader: 'babel-loader',
            query: {
              presets: ['env']
            }
          }
        ]
      }
    ]
  },
}]
