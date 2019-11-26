
import os
import sys

cmd = sys.argv[1]

if cmd == "version":
    source = sys.argv[2]
    target = sys.argv[3]

    f = open(source, "r")
    l = f.readline()
    f.close()

    if l.endswith("\n"):
        l = l[:len(l) - 1]

    t = open(target, "w")
    t.writelines([
        "/* this file is auto generated */\n",
        "#ifndef JSB_VERSION_H\n", 
        "#define JSB_VERSION_H\n", 
        "#define CONFIG_VERSION \"" + l + "\"\n", 
        "#endif\n", 
    ])
    t.close()
else:
    print "unknown command"
    