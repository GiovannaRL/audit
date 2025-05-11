xPlanner.factory('FileService', ['AuthService', function (AuthService) {

    var _downloadFile = function (url, filename) {

        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader("Authorization", "Bearer " + AuthService.getAccessToken());
        xhr.responseType = "blob";
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                var a = document.createElement('a');
                a.href = window.URL.createObjectURL(xhr.response); // xhr.response is a blob
                a.download = filename; // Set the file name.
                a.style.display = 'none';
                document.body.appendChild(a);
                a.click();
                //delete a; 
            }
        };
        xhr.send(null);
    };

    var _getBase64 = function (file) {
        return new Promise(function (resolve, reject) {
            const reader = new FileReader();
            reader.readAsDataURL(file);
            reader.onload = function () {
                var idx = reader.result.indexOf('base64,');
                if (idx >= 0) {
                    resolve(reader.result.substring(idx + 7));
                } else {
                    resolve(reader.result);
                }
            };
            reader.onerror = function (error) { reject(error); };
        });
    };

    var _imageFileSizeLimit = 7; // megabytes

    var _getExtensionAndName = function (fullName) {

        if (!fullName) return null;

        var splited = fullName.split('.');

        return {
            extension: splited[splited.length - 1],
            name: splited.slice(0, splited.length - 1).join()
        };
    };

    var _getBase64FileFirefox = function (elemId) {

        return new Promise(function (resolve, reject) {
            var file = document.getElementById(elemId).files;
            if (file && file.length > 0) {
                _getBase64(file[0]).then(function (base64File) {
                    var extensionName = _getExtensionAndName(file[0].name);

                    resolve({
                        fileExtension: extensionName.extension,
                        fileName: extensionName.name,
                        base64File: base64File
                    });
                }, function (error) { reject(error); });
            } else {
                resolve(null);
            }
        });
    };

    var _getBase64NoFileFirefox = function (file) {
        return new Promise(function (resolve, reject) {
            if (file) {
                _getBase64(file).then(function (base64File) {
                    var extensionName = _getExtensionAndName(file.name);

                    resolve({
                        fileExtension: extensionName.extension,
                        fileName: extensionName.name,
                        base64File: base64File
                    });
                }, function (error) { reject(error); });
            } else {
                resolve(null);
            }
        });
    };

    return {
        GetBase64NoFileFirefox: _getBase64NoFileFirefox,
        GetBase64FileFirefox: _getBase64FileFirefox,
        GetExtensionAndName: _getExtensionAndName,
        ImageFileSizeLimit: _imageFileSizeLimit,
        GetBase64: _getBase64,
        DownloadFile: _downloadFile
    };

}]);