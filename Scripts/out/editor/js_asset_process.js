["OnPostprocessTexture", "OnPostprocessModel", "OnPostprocessAudio", "OnPostprocessMaterial", "OnPostprocessAllAssets"].forEach(k => {
    globalThis[k] = function () {
        const p = require("./asset_importer")[k];
        if (p) {
            try {
                p(...arguments);
            }
            catch (e) {
                console.error(e);
            }
        }
    };
});
//# sourceMappingURL=js_asset_process.js.map