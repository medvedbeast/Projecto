function OnCreateAttachmentClicked() {
    var input = $("#attachments-input");
    $(input).click();
}

function OnAttachmentInputChanged() {
    var input = $("#attachments-input")[0];

    if (input.files.length > 0) {
        var file = input.files[0];

        var reader = new FileReader();
        reader.onload = function () {
            var image = new Image();
            image.onload = function () {
                var maxWidth = 1920;
                var maxHeight = 1080;
                var imageUrl = image.src;

                if (image.width > maxWidth || image.height > maxHeight) {
                    var canvas = document.getElementById("attachment-canvas");
                    var context = canvas.getContext("2d");
                    context.clearRect(0, 0, canvas.width, canvas.height);
                    context.drawImage(image, 0, 0);

                    var width = image.width;
                    var height = image.height;

                    if (width > height) {
                        if (width > maxWidth) {
                            height *= maxWidth / width;
                            width = maxWidth;
                        }
                    } else {
                        if (height > maxHeight) {
                            width *= maxHeight / height;
                            height = maxHeight;
                        }
                    }
                    canvas.width = width;
                    canvas.height = height;

                    context = canvas.getContext("2d");
                    context.drawImage(image, 0, 0, width, height);

                    imageUrl = canvas.toDataURL("image/png");
                }

                $("#attachment-create-dialog").find("img").attr("src", imageUrl);
                $("#attachment-create-dialog").find("textarea").val(" ");
                ShowDialog("#attachment-create-dialog");

                $("#attachment-input-form")[0].reset();
            };
            image.src = reader.result;
        };
        reader.readAsDataURL(file);
    }
    $("#attachment-input-form")[0].reset();
}

function OnConfirmCreateAttachmentClicked() {

    var image = $("#attachment-create-dialog img").attr("src");
    var comment = $("#attachment-create-dialog textarea").val();
    var action = $("#attachments-input").attr("create-action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Content: image,
            Comment: comment
        }),
        success: function (response) {
            HideDialog("#attachment-create-dialog");
            if (response.status === true) {
                location.href = response.url;
            }
            else {
                ShowDialog("#error-dialog");
            }
        },
        error: function (response) {
            ShowDialog("#error-dialog");
            console.log(response);
        }
    });
}

var selectedAttachment = 0;

function OnViewAttachmentClicked(attachmentId) {
    selectedAttachment = attachmentId;
    var comment = $(`#attachments .item#${attachmentId} .data`).attr("comment");
    var image = $(`#attachments .item#${attachmentId}`).css("background-image").substring(5);
    image = image.substring(0, image.length - 2);

    $("#attachment-view-dialog img").attr("src", image);
    $("#attachment-view-dialog .input div").html(comment);

    ShowDialog("#attachment-view-dialog");
}

function OnDeleteAttachmentClicked() {
    var id = selectedAttachment;
    var action = $("#attachment-view-dialog form").attr("delete-action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: id
        }),
        success: function (response) {
            HideDialog("#attachment-view-dialog");
            if (response.status === true) {
                ShowDialog("#success-dialog");
                $(`#attachments .item#${id}`).detach();
            }
            else {
                ShowDialog("#error-dialog");
            }
        },
        error: function (response) {
            ShowDialog("#error-dialog");
            console.log(response);
        }
    });
}

function OnEnlargeAttachmentClicked() {
    var image = $("#attachment-view-dialog img").attr("src");
    var w = window.open("", "_blank");
    w.document.write(`<img src='${image}' />`);
}