mergeInto(LibraryManager.library, {
    RegisterBeforeUnloadSave: function(gameObjectNamePtr, callbackMethodPtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var callbackMethod = UTF8ToString(callbackMethodPtr);

        if (window.__runPersBeforeUnloadSaveRegistered) {
            return;
        }
        window.__runPersBeforeUnloadSaveRegistered = true;

        window.addEventListener("beforeunload", function() {
            if (typeof SendMessage !== "undefined") {
                SendMessage(gameObjectName, callbackMethod);
            }
        });
    }
});
