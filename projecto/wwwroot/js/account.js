function OnInviteClicked(id) {
    var form = $(id + " form");

    var action = form.attr("action");
    var targetEmail = form.find("input#Email").val();

    var data = {
        Email: targetEmail
    };

    if (form.valid()) {
        $.ajax({
            url: action,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (response) {
                if (response.status === true) {
                    HideDialog(id);
                    ShowDialog("#answer-dialog");
                }
            },
            error: function (response) {
                console.log(response);
            }
        });
    }
}

function OnConfirmDeleteClicked(id) {
    var form = $(id + " form");

    var action = form.attr("action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        success: function (response) {
            if (response.status === true) {
                HideDialog(id);
                ShowDialog("#answer-dialog");
            }
        },
        error: function (response) {
            console.log(response);
        }
    });
}