let timer;


function validateEmailField(inputId, emailLoaderId, errorId, successId) {

    $("#" + inputId).on("input", function () {

        clearTimeout(timer);

        let input = $(this);
        let email = input.val();

        $("#" + errorId).text("");
        $("#" + successId).text("");

        input.removeClass("is-invalid is-valid");

        if (email.trim() === "") {
            return;
        }

        timer = setTimeout(function () {

            $("#" + emailLoaderId).removeClass("d-none");

            $.ajax({
                url: '/Account/ValidateEmail',
                type: 'POST',
                data: { email: email },

                success: function (response) {

                    $("#" + emailLoaderId).addClass("d-none");

                    if (response === true || response === "true") {

                        input
                            .removeClass("is-invalid")
                            .addClass("is-valid");

                        $("#" + successId)
                            .text("Valid email address");

                        $("#" + errorId).text("");
                    }
                    else {
                        $("#emailLoader").addClass("d-none");
                        input
                            .removeClass("is-valid")
                            .addClass("is-invalid");

                        $("#" + errorId).text(response);

                        $("#" + successId).text("");
                    }
                },

                error: function () {
                    $("#" + emailLoaderId).addClass("d-none");
                    input.removeClass("is-valid")
                        .addClass("is-invalid");

                    $("#" + errorId).text("Something went wrong");
                }
            });

        }, 500);

    });
}


$("#EmailID").on("input", function () {

    clearTimeout(timer);
    let email = $(this).val();

    $("#emailError").text("");
    $("#emailSuccess").text("");

    // Remove old styles
    $(this).removeClass("is-invalid is-valid");

    // Empty check
    if (email.trim() === "") {
        return;
    }

    // Debounce
    timer = setTimeout(function () {
        $("#emailLoader").removeClass("d-none");

        $.ajax({
            url: '/Account/ValidateEmail',
            type: 'POST',
            data: { email: email },
            success: function (response) {

                $("#emailLoader").addClass("d-none");

                if (response === true || response === "true") {
                    $("#EmailID").removeClass("is-invalid")
                        .addClass("is-valid");

                    $("#emailSuccess").text("Valid email address");
                    $("#emailError").text("");
                }
                else {
                    $("#EmailID").removeClass("is-valid")
                        .addClass("is-invalid");

                    $("#emailError").text(response);
                    $("#emailSuccess").text("");
                }
            },

            error: function () {
                $("#emailLoader").addClass("d-none");
                $("#EmailID").removeClass("is-valid")
                    .addClass("is-invalid");
                $("#emailError").text("Something went wrong");
            }
        });

    }, 500);

});


$("#ParentEmail").on("input", function () {

    clearTimeout(timer);

    let email = $(this).val();

    $("#fatherEmailError").text("");
    $("#fatherEmailSuccess").text("");

    // Remove old styles
    $(this).removeClass("is-invalid is-valid");

    // Empty check
    if (email.trim() === "") {
        return;
    }

    // Debounce
    timer = setTimeout(function () {

        $("#fatherEmailLoader").removeClass("d-none");

        $.ajax({
            url: '/Account/ValidateEmail',
            type: 'POST',
            data: { email: email },

            success: function (response) {

                $("#fatherEmailLoader").addClass("d-none");

                if (response === true || response === "true") {

                    $("#ParentEmail")
                        .removeClass("is-invalid")
                        .addClass("is-valid");

                    $("#fatherEmailSuccess").text("Valid email address");
                    $("#fatherEmailError").text("");
                }
                else {

                    $("#ParentEmail")
                        .removeClass("is-valid")
                        .addClass("is-invalid");

                    $("#fatherEmailError").text(response);
                    $("#fatherEmailSuccess").text("");
                }
            },

            error: function () {

                $("#fatherEmailLoader").addClass("d-none");

                $("#ParentEmail")
                    .removeClass("is-valid")
                    .addClass("is-invalid");

                $("#fatherEmailError").text("Something went wrong");
            }
        });

    }, 500);

});

function validateAadhaarField(inputId, errorId, successId) {

    $("#" + inputId).on("input", function () {

        let input = $(this);

        // Allow only digits
        input.val(input.val().replace(/\D/g, ''));

        let aadhaar = input.val();

        $("#" + errorId).text("");
        $("#" + successId).text("");

        input.removeClass("is-valid is-invalid");

        if (aadhaar.length < 12) {
            return;
        }

        $.ajax({
            url: '/Account/ValidateAadhar',
            type: 'POST',
            data: { AadhaarNumber: aadhaar },
            success: function (response) {

                if (response === true || response === "true") {

                    input
                        .removeClass("is-invalid")
                        .addClass("is-valid");

                    $("#" + successId)
                        .text("Valid Aadhaar number");

                } else {

                    input
                        .removeClass("is-valid")
                        .addClass("is-invalid");

                    $("#" + errorId).text(response);
                }
            }
        });

    });
}

function validateMobileNoField(inputId, errorId, successId) {

    $("#" + inputId).on("input", function () {

        let input = $(this);

        // Allow only digits
        input.val(input.val().replace(/\D/g, ''));

        let mobileNo = input.val();

        $("#" + errorId).text("");
        $("#" + successId).text("");

        input.removeClass("is-valid is-invalid");

        if (mobileNo.length < 10) {
            return;
        }

        $.ajax({
            url: '/Account/ValidatePhoneNumber',
            type: 'POST',
            data: { phoneNumber: mobileNo },
            success: function (response) {

                if (response === true || response === "true") {

                    input
                        .removeClass("is-invalid")
                        .addClass("is-valid");

                    $("#" + successId)
                        .text("Valid mobile number");

                } else {

                    input
                        .removeClass("is-valid")
                        .addClass("is-invalid");

                    $("#" + errorId).text(response);
                }
            }
        });

    });
}