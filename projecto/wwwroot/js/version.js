function OnConfirmDeleteClicked(id) {
    var form = $(id + " form");

    var action = form.attr("action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        success: function (response) {
            if (response.status === true) {
                window.location = response.url;
            }
            else {
                HideDialog("#delete-dialog");
                ShowDialog("#answer-dialog");
            }
        },
        error: function (response) {
            console.log(response);
        }
    });
}