function ShowDialog(id) {
    var dialog = $(id);
    dialog.find("form")[0].reset();
    dialog.find(".error span").each(function () {
        $(this).text(" ");
    });
    dialog.toggleClass("hidden");
}

function HideDialog(id) {
    var dialog = $(id);
    dialog.addClass("hidden");
}