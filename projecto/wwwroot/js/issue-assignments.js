function OnKeywordsChanged() {
    var keywords = $("#create-dialog input[name='Keywords']").val().split(" ");

    if (keywords.length != 0) {
        $("#create-dialog #search-results input").each(function () {
            var flag = false;
            var label = $(this).next("label").html().toLowerCase();
            keywords.forEach(function (item, i, arr) {
                if (label.includes(item)) {
                    flag = true;
                }
            });
            if (!flag) {
                $(this).parent().addClass("hidden");
            }
            else {
                $(this).parent().removeClass("hidden");
            }
        });
    } else {
        $("#create-dialog #search-results input").each(function () {
            $(this).parent().removeClass("hidden");
        });
    }
}

function OnConfirmCreateClicked() {
    var role = $("#create-dialog #assignment-role").val();
    var idList = [];
    $("#create-dialog #search-results input:checked").each(function () {
        idList.push($(this).val());
    });
    if (idList.length < 1) {
        return;
    }
    var action = $("#create-dialog form").attr("action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            RoleId: role,
            UserIdList: idList
        }),
        success: function (response) {
            if (response.status === true) {
                location.href = response.url;
            } else {
                ShowDialog("#error-dialog");
            }
        },
        error: function (response) {
            ShowDialog("#error-dialog");
            console.log(response);
        }
    });
}

var selectedAssignment = 0;

function OnDeleteAssignmentClicked(id) {
    selectedAssignment = id;
    ShowDialog("#delete-dialog");
}

function OnConfirmDeleteClicked() {
    var id = selectedAssignment;
    var action = $("#delete-dialog form").attr("action")

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: id
        }),
        success: function (response) {
            if (response.status === true) {
                location.href = response.url;
            } else {
                ShowDialog("#error-dialog");
            }
        },
        error: function (response) {
            ShowDialog("#error-dialog");
            console.log(response);
        }
    });
}

function OnUpdateAssignmentClicked(id) {
    selectedAssignment = id;
    var name = $("#content .item#" + id + " .data").attr("name");
    var roleId = $("#content .item#" + id + " .data").attr("role-id");

    $("#update-dialog form .content").html("Update project role of <b>" + name + "</b>:");
    $("#update-dialog form select option:selected").attr("selected", false);
    $("#update-dialog form select option[value='" + roleId + "']").attr("selected", true);

    ShowDialog("#update-dialog");
}

function OnConfirmUpdateClicked() {
    var id = selectedAssignment;
    var roleName = $("#content .item#" + id + " .data").attr("name");
    var roleId = $("#update-dialog form select option:selected").attr("value");
    var action = $("#update-dialog form").attr("action");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            RoleId: roleId,
            AssignmentId: id
        }),
        success: function (response) {
            if (response.status === true) {
                location.href = response.url;
            } else {
                ShowDialog("#error-dialog");
            }
        },
        error: function (response) {
            ShowDialog("#error-dialog");
            console.log(response);
        }
    });
}