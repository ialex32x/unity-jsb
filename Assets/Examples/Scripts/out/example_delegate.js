let actions = new jsb.DelegateTest();
console.log("********** add");
actions.onAction("add", function () {
    console.log("js action call");
});
console.log("********** call");
actions.CallAction();
actions.onAction("set", null);
console.log("********** after clear, call again");
actions.CallAction();
//# sourceMappingURL=example_delegate.js.map