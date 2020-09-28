"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.genCode = void 0;
const csharp_1 = require("csharp");
const CodeWriter_1 = require("./CodeWriter");
function genBinder(writer, codePkgName, classes, classCnt, fuiNamespace, exportCodePath) {
    let binderName = codePkgName + 'Binder';
    for (let i = 0; i < classCnt; i++) {
        let classInfo = classes.get_Item(i);
        writer.writeln('import %s from "./%s";', classInfo.className, classInfo.className);
    }
    // if (isUnity) {
    //     writer.writeln('import * as fgui from "fairygui-jsb";');
    //     writer.writeln();
    // }
    writer.writeln();
    writer.writeln('export default class %s', binderName);
    writer.startBlock();
    writer.writeln('public static bindAll():void');
    writer.startBlock();
    for (let i = 0; i < classCnt; i++) {
        let classInfo = classes.get_Item(i);
        writer.writeln('%s.UIObjectFactory.SetPackageItemExtension(%s.URL, () => new %s());', fuiNamespace, classInfo.className, classInfo.className);
    }
    writer.endBlock(); //bindall
    writer.endBlock(); //class
    writer.save(exportCodePath + '/' + binderName + '.ts');
}
function getSafeVarName(name) {
    return name.replace("&", "And");
}
function genCode(handler) {
    let settings = handler.project.GetSettings("Publish").codeGeneration;
    let codePkgName = handler.ToFilename(handler.pkg.name); //convert chinese to pinyin, remove special chars etc.
    let exportCodePath = handler.exportCodePath + '/' + codePkgName;
    let namespaceName = codePkgName;
    let fuiNamespace = "FairyGUI";
    let isUnity = handler.project.type == csharp_1.FairyEditor.ProjectType.Unity;
    if (settings.packageName)
        namespaceName = settings.packageName + '.' + namespaceName;
    //CollectClasses(stripeMemeber, stripeClass, fguiNamespace)
    let classes = handler.CollectClasses(settings.ignoreNoname, settings.ignoreNoname, fuiNamespace);
    handler.SetupCodeFolder(exportCodePath, "ts"); //check if target folder exists, and delete old files
    let getMemberByName = settings.getMemberByName;
    let classCnt = classes.Count;
    let writer = new CodeWriter_1.default({ blockFromNewLine: false, usingTabs: true });
    for (let i = 0; i < classCnt; i++) {
        let classInfo = classes.get_Item(i);
        let members = classInfo.members;
        let references = classInfo.references;
        writer.reset();
        let refCount = references.Count;
        if (refCount > 0) {
            for (let j = 0; j < refCount; j++) {
                let ref = references.get_Item(j);
                writer.writeln('import %s from "./%s";', ref, ref);
            }
            writer.writeln();
        }
        if (isUnity) {
            // writer.writeln('import * as fgui from "fairygui-jsb";');
            // if (refCount == 0)
            //     writer.writeln();
        }
        writer.writeln('export default class %s', classInfo.className);
        writer.startBlock();
        writer.writeln('public gRoot: %s', classInfo.superClassName);
        writer.writeln();
        let memberCnt = members.Count;
        for (let j = 0; j < memberCnt; j++) {
            let memberInfo = members.get_Item(j);
            writer.writeln('public %s: %s;', getSafeVarName(memberInfo.varName), memberInfo.type);
        }
        writer.writeln('public static URL: string = "ui://%s%s";', handler.pkg.id, classInfo.resId);
        writer.writeln();
        writer.writeln('public static createInstance(): %s', classInfo.className);
        writer.startBlock();
        writer.writeln('let inst = new %s();', classInfo.className);
        writer.writeln('inst.gRoot = <%s>(%s.UIPackage.CreateObject("%s", "%s"));', classInfo.superClassName, fuiNamespace, handler.pkg.name, classInfo.resName);
        writer.writeln('inst.onConstruct();');
        writer.writeln('return inst;');
        writer.endBlock();
        writer.writeln();
        writer.writeln('public static fromInstance(gRoot: %s.GObject): %s', fuiNamespace, classInfo.className);
        writer.startBlock();
        writer.writeln('    let inst = new %s();', classInfo.className);
        writer.writeln('    inst.gRoot = <%s>gRoot;', classInfo.superClassName);
        writer.writeln('    inst.onConstruct();');
        writer.writeln('    return inst;');
        writer.endBlock();
        writer.writeln();
        writer.writeln('protected onConstruct(): void');
        writer.startBlock();
        for (let j = 0; j < memberCnt; j++) {
            let memberInfo = members.get_Item(j);
            var memberVarName = getSafeVarName(memberInfo.varName);
            if (memberInfo.group == 0) {
                if (memberInfo.type.startsWith(fuiNamespace)) {
                    if (getMemberByName) {
                        writer.writeln('this.%s = <%s>(this.gRoot.GetChild("%s"));', memberVarName, memberInfo.type, memberInfo.name);
                    }
                    else {
                        writer.writeln('this.%s = <%s>(this.gRoot.GetChildAt(%s));', memberVarName, memberInfo.type, memberInfo.index);
                    }
                }
                else {
                    if (getMemberByName) {
                        writer.writeln('this.%s = %s.fromInstance(this.gRoot.GetChild("%s"));', memberVarName, memberInfo.type, memberInfo.name);
                    }
                    else {
                        writer.writeln('this.%s = %s.fromInstance(this.gRoot.GetChildAt(%s));', memberVarName, memberInfo.type, memberInfo.index);
                    }
                }
            }
            else if (memberInfo.group == 1) {
                if (getMemberByName) {
                    writer.writeln('this.%s = this.gRoot.GetController("%s");', memberVarName, memberInfo.name);
                }
                else {
                    writer.writeln('this.%s = this.gRoot.GetControllerAt(%s);', memberVarName, memberInfo.index);
                }
            }
            else {
                if (getMemberByName) {
                    writer.writeln('this.%s = this.gRoot.GetTransition("%s");', memberVarName, memberInfo.name);
                }
                else {
                    writer.writeln('this.%s = this.gRoot.GetTransitionAt(%s);', memberVarName, memberInfo.index);
                }
            }
        }
        writer.endBlock();
        writer.endBlock(); //class
        writer.save(exportCodePath + '/' + classInfo.className + '.ts');
    }
    writer.reset();
}
exports.genCode = genCode;
