"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const _ = require("lodash");
let greet = function (greeting, punctuation) {
    return greeting + ' ' + this.user + punctuation;
};
let object = { 'user': 'fred' };
let bound1 = _.bind(greet, object, 'hi');
console.log(bound1('!'));
// => 'hi fred!'
// Bound with placeholders.
let bound2 = _.bind(greet, object, _, '!');
console.log(bound2('hi'));
// => 'hi fred!'
console.log(_.upperCase('--foo-bar'));
console.log(_.snakeCase('--FOO-BAR--'));
console.log(_.deburr('déjà vu'));
//# sourceMappingURL=example_lodash.js.map