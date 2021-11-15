
declare class FSWatcher {
    constructor(path?: string, filter?: string);

    path: string;
    filter: string;
    enableRaisingEvents: boolean;
    includeSubdirectories: boolean;

    oncreate: (name: string, fullPath: string) => void;
    ondelete: (name: string, fullPath: string) => void;
    onchange: (name: string, fullPath: string) => void;

    dispose(): void;
}
