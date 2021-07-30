"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.OnPostprocessAllAssets = exports.OnPostProcessSprites = exports.OnPostprocessMaterial = exports.OnPostprocessAudio = exports.OnPostprocessModel = exports.OnPostprocessTexture = void 0;
// You need to register this script into assetPostProcessors in js-bridge.json (or Prefs.cs) to activate these callbacks
function OnPostprocessTexture(processor, tex) {
    // console.log("OnPostprocessTexture", processor.assetPath);
}
exports.OnPostprocessTexture = OnPostprocessTexture;
function OnPostprocessModel(processor, model) {
    // console.log("OnPostprocessModel", processor.assetPath);
}
exports.OnPostprocessModel = OnPostprocessModel;
function OnPostprocessAudio(processor, audioClip) {
    // console.log("OnPostprocessAudio", processor.assetPath);
}
exports.OnPostprocessAudio = OnPostprocessAudio;
function OnPostprocessMaterial(processor, material) {
    // console.log("OnPostprocessMaterial", processor.assetPath);
}
exports.OnPostprocessMaterial = OnPostprocessMaterial;
function OnPostProcessSprites(processor, texture, sprites) {
    // console.log("OnPostProcessSprites", processor.assetPath);
}
exports.OnPostProcessSprites = OnPostProcessSprites;
function OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths) {
}
exports.OnPostprocessAllAssets = OnPostprocessAllAssets;
//# sourceMappingURL=asset_postprocessor.js.map