import { AsyncTaskTest } from "Example";
import { Yield } from "jsb";
import { IPHostEntry } from "System.Net";
import { WaitForSeconds, Time } from "UnityEngine";

function delay(secs) {
    return new Promise<number>((resolve, reject) => {
        setTimeout(() => {
            print("[async] resolve");
            resolve(123);
        }, secs * 1000);
    });
}

async function test() {
    print("[async] begin");
    await delay(3);
    print("[async] end");
    let result = <IPHostEntry>await Yield(AsyncTaskTest.GetHostEntryAsync("www.baidu.com"));
    console.log("host entry:", result.HostName);
}

async function testUnityYieldInstructions() {
    console.warn("wait for unity YieldInstruction, begin");
    await Yield(new WaitForSeconds(3));

    console.warn("wait for unity YieldInstruction, end;", Time.frameCount);
    await Yield(null);
    console.warn("wait for unity YieldInstruction, next frame;", Time.frameCount);
}

test();
testUnityYieldInstructions();
