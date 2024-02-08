Sys.IDisposable = function () {
  ##DEBUG throw Error.notImplemented();
};
Sys.IDisposable.prototype = {
  #if DEBUG
  dispose: function () {
    throw Error.notImplemented();
  },
  #endif
};
Sys.IDisposable.registerInterface("Sys.IDisposable");
