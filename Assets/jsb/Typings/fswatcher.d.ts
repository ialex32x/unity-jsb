
declare class FSWatcher {
    constructor(path?: string, filter?: string);

    path: string;
    filter: string;
    enableRaisingEvents: boolean;
    includeSubdirectories: boolean;

    oncreate: (name: string) => void;
    ondelete: (name: string) => void;
    onchange: (name: string) => void;

    dispose(): void;
}
