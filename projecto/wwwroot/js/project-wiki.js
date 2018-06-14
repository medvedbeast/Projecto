function OnBodyLoaded() {
    $("#page-2 textarea").trumbowyg({
        autogrow: true
    });
}

function OnTabClicked(index) {
    $(".tabs .item").removeClass("selected");
    $(`.tabs .item[tabindex=${index}]`).addClass("selected");

    $(".pages .item").removeClass("selected");
    $(`.pages .item[tabindex='${index}']`).addClass("selected");
}

function OnUpdateClicked() {
    var content = $("#page-2 .trumbowyg-editor").html();
    var action = $("#page-2 .data").attr("action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Content: content
        }),
        success: function (response) {
            if (response.status === true) {
                ShowDialog("#success-dialog");
                wiki.content = content;
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