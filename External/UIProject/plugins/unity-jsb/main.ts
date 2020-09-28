import { FairyEditor } from 'csharp';
import { genCode } from './GenCode_TS';

function onPublish(handler: FairyEditor.PublishHandler) {
    if (!handler.genCode) return;
    handler.genCode = false; //prevent default output

    console.log('Handling gen code in plugin 111');
    genCode(handler); //do it myself
}

function onDestroy() {
    //do cleanup here
}

export { onPublish, onDestroy };