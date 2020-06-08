# unity-jsb

# compile for windows on linux
sudo apt-get update
sudo apt-get install mingw-w64

./configure --host=i686-w64-mingw32
x86_64-w64-mingw32


# TODO
* [X] console.* compatible
* sourcemap helper
* event dispatcher
* ref 传参时, 从 val.target 进行取值 (因为会需要回写target, 保持一致性)
* Values_push_class.cs ```public static JSValue js_push_classvalue(JSContext ctx, IO.ByteBuffer o)```
* 