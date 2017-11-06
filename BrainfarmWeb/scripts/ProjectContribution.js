$(document).ready(function () { 

    // "Is Contribution Comment" checkbox click
    $(document).on("click", ".chk-is-contribution", function () {
        var checkbox = $(event.target);
        var divFileUpload = $(".div-file-upload");
        divFileUpload.toggleClass("reply-box-hidden", checkbox.checked);
    });

    // "Add a file" button clicked
    $(document).on("click", ".btn-add-file", function () {
        var control = "<input type='file' class='input-file-upload btn btn-green' />";
        var divFileUploadInputs = $(".div-file-upload-inputs");
        divFileUploadInputs.append(control);
        divFileUploadInputs.append("<br />");

        return false; //Prevent ASP.NET postback
    });

    // File selected
    $(document).on("change", ".file-upload", function () {
        var fileInputControl = $(event.target);
        var fileInputLabel = $(".file-upload-label");

        pendingFileUploads = [];
        var requests = [];
        var labelText = "";

        for (var i = 0; i < fileInputControl[0].files.length; i++) {
            var file = fileInputControl[0].files[i];

            // Build label text
            if (i > 0)
                labelText += ", ";
            labelText += file.name;

            // start the upload and add it to list of requests
            requests.push(uploadFile(file))
        }

        fileInputLabel.text("Uploading...")

        $.when.apply(undefined, requests).then(function () {
            fileInputLabel.text(labelText);
        });
    });

    // Contribution file link clicked
    $(document).on("click", ".attachments a", attachmentClicked);

});

function uploadFile(file) {
    return $.ajax({
        url: serviceEndpoint + "UploadFile",
        type: "POST",
        data: file,
        processData: false,
        success: function (response) {
            console.log("upload done");
            pendingFileUploads.push({
                ContributionFileID: response.ContributionFileID,
                Filename: file.name
            });
        },
        error: function () {
            console.log("upload error");
        }
    });
}

function attachmentClicked(event) {
    // Get file id and filename from clicked tag
    var contributionFileId = $(event.target).data("contribution-file-id");
    var filename = $(event.target).data("filename");

    // Direct to download controller
    location.href = "/DownloadFile.ashx?ID=" + contributionFileId + "&Filename=" + filename;

}
