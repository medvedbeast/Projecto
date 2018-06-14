var dragged = null;
var draggedParent = null;

function OnCardDragStarted(event, card) {
    event.dataTransfer = null;
    dragged = $(card);
    draggedParent = $(card).parent();
}

function OnCardDragEnded(card) {
    $(".container .placeholder").each(function () {
        $(this).detach();
    });
}

function OnCardDragEnter(event, container) {
    if (draggedParent[0] != $(container)[0]) {
        $(event.target).addClass("highlighted");
    }
    else {
        event.preventDefault();
    }
}

function OnCardDragLeave(event) {
    $(event.target).removeClass("highlighted");
}

function OnCardDropped(event, container) {
    if (draggedParent[0] != $(container)[0]) {
        event.preventDefault();
        $(container).removeClass("highlighted");
        Update(container);
    }
    $(container).removeClass("highlighted");
}

function AllowDrop(event) {
    event.preventDefault();
}

function Update(container) {
    var action = $("#content").attr("url");
    var issueId = $(dragged).attr("issue-id");
    var statusId = $(container).attr("status-id");

    $.ajax({
        url: action,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            IssueId: issueId,
            StatusId: statusId
        }),
        success: function (response) {
            if (response.status === true) {
                Append(dragged.clone(), container);
                $(dragged).detach();
                dragged = null;
                draggedParent = null;
            }
            else {
                dragged = null;
                draggedParent = null;
            }
        },
        error: function (response) {
            console.log(response);
        }
    });
}

function Append(element, container) {
    var target = null;
    var id = $(element).attr("issue-id");
    var items = $(container).find(".item");

    if (items.length > 0) {
        items.each(function () {
            var currentId = $(this).attr("issue-id");
            if (currentId < id) {
                target = this;
            }
        });
        if (target != null) {
            target.after(element[0]);
        }
        else {
            items[0].before(element[0]);
        }
        
    }
    else {
        container.append(element[0]);
        return;
    }
}