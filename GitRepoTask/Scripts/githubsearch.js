$(document).ready(function () {
    $("form").submit(function (e) {
    });

    $("#ajaxSearchButton").click(function (e) {
        e.preventDefault();
        performAjaxSearch();
    });

    $("#SearchModel_Username").keypress(function (e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            performAjaxSearch();
        }
    });

    function performAjaxSearch() {
        if (!$("#searchForm").valid()) {
            return;
        }

        var username = $("#SearchModel_Username").val();

        $("#loadingIndicator").show();
        $("#ajaxSearchButton").prop('disabled', true);
        $("#profileContainer").hide();
        $("#errorMessage").hide();

        $.ajax({
            url: "/Home/GetProfilePartial",
            type: "GET",
            data: { username: username },
            success: function (result) {
                $("#profileContainer").html(result).show();
                $("#ajaxSearchButton").prop('disabled', false);
                $("#loadingIndicator").hide();
            },
            error: function (xhr) {
                var message = "An unexpected error occurred! Please try again later.";

                if (xhr.status === 400) {
                    console.log(xhr);
                    message = xhr.responseText;
                } else if (xhr.status === 404) {
                    console.log(xhr);
                    message = xhr.responseText;
                }

                $("#errorMessage").text(message).show();
                $("#ajaxSearchButton").prop('disabled', false);
                $("#loadingIndicator").hide();
            }
        });
    }
});