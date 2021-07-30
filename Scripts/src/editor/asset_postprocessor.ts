
import { Array } from "System";
import { AssetPostprocessor } from "UnityEditor";
import { AudioClip, GameObject, Material, Sprite, Texture2D } from "UnityEngine";

// You need to register this script into assetPostProcessors in js-bridge.json (or Prefs.cs) to activate these callbacks

export function OnPostprocessTexture(processor: AssetPostprocessor, tex: Texture2D) {
    // console.log("OnPostprocessTexture", processor.assetPath);
}

export function OnPostprocessModel(processor: AssetPostprocessor, model: GameObject) {
    // console.log("OnPostprocessModel", processor.assetPath);
}

export function OnPostprocessAudio(processor: AssetPostprocessor, audioClip: AudioClip) {
    // console.log("OnPostprocessAudio", processor.assetPath);
}

export function OnPostprocessMaterial(processor: AssetPostprocessor, material: Material) {
    // console.log("OnPostprocessMaterial", processor.assetPath);
}

export function OnPostProcessSprites(processor: AssetPostprocessor, texture: Texture2D, sprites: Array<Sprite>) {
    // console.log("OnPostProcessSprites", processor.assetPath);
}

export function OnPostprocessAllAssets(importedAssets: Array<string>, deletedAssets: Array<string>, movedAssets: Array<string>, movedFromAssetPaths: Array<string>) {
    
}
