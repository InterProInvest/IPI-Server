$(document).ready(function () {
    $('.loading').removeClass('d-flex');
    $('.loading').addClass('d-none');

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        ToggleSidebarClass();
    }

    $('#collapseAudit').on('show.bs.collapse', function () {
        $('#collapseSettings').collapse('hide');
    });

    $('#collapseSettings').on('show.bs.collapse', function () {
        $('#collapseAudit').collapse('hide');
    });
});

function ToggleSidebar() {
    ToggleSidebarClass();

    var toggled = localStorage.getItem('ToggledSidebar');
    if (toggled === 'true') {
        localStorage.setItem('ToggledSidebar', 'false');
    }
    else {
        localStorage.setItem('ToggledSidebar', 'true');
    }
}

function ToggleSidebarClass() {
    $('.custom-sidebar').toggleClass('toggled');
    $('.page_labels').toggleClass('toggled');
    $('.sidebar-settings').toggleClass('toggled');
    $('.icon-arrow').toggleClass('toggled');
    $('.copyright').toggleClass('toggled');
    $('.loading').toggleClass('toggled');
    $('.icon-hideez').toggleClass('toggled');
}

function createBreadcrumbs(breadcrumbs) {
    $('#breadcrumb').toggleClass('d-none');
    for (var i = 0; i < breadcrumbs.length; i++) {
        if (breadcrumbs[i].active) {
            $('.breadcrumb').append('<li class="breadcrumb-item active">' + breadcrumbs[i].content + '</li>');
        }
        else {
            $('.breadcrumb').append('<li class="breadcrumb-item"><a href="' + breadcrumbs[i].link + '">' + breadcrumbs[i].content + '</a ></li>');
        }
    }
}

function showSpinner(elementId) {
    $('#' + elementId).toggleClass('d-none');
}

function toggleModalDialog(dialogId) {
    $('#' + dialogId).modal('toggle');
}

function triggerCheckbox(checkboxId) {
    var chkb = $('#' + checkboxId);
    chkb.trigger('click');
}

function toggleCheckbox(checkboxId) {
    var chkb = $('#' + checkboxId);
    chkb.attr("checked", !chkb.attr("checked"));
}

function toggleAllCheckboxes(tableId, state) {
    $('#' + tableId).find('input[type="checkbox"]').each(function () {
        $(this).attr('checked', state);
    });
}

function copyToClipboard() {
    $("#activationCodeInput").attr("type", "Text").select();
    document.execCommand("copy");
    $("#activationCodeInput").attr("type", "Password").select();
}