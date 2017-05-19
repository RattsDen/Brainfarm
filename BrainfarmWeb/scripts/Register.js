$(document).ready(function () {

    $(document).on("click", "[id$=btnRegister]", function (event) {
        if (!(passwordFieldsMatch() && passwordMeetsRequirements())) {
            event.preventDefault();
        }
    });

});

function passwordFieldsMatch() {
    return $("[id$=txtPassword]").val() == $("[id$=txtPasswordConfirm]").val()
}

function passwordMeetsRequirements() {
    return $("[id$=txtPassword]").val().length >= 8;
}
