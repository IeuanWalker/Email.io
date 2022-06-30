function loadToests(message, status) {
    document.addEventListener("DOMContentLoaded", function () {
        var toastMessage = message;
        var toastStatus = status;

        if (toastMessage != "" && message != null) {
            var options = {
                // STRING: main class name used to styling each toast message with CSS:
                // .... IMPORTANT NOTE:
                // .... if you change this, the configuration consider that you´re
                // .... re-stylized the plug-in and default toast styles, including CSS3 transitions are lost.
                classname: "toast",
                // STRING: name of the CSS transition that will be used to show and hide all toast by default:
                transition: "slideRightFade",
                // BOOLEAN: specifies the way in which the toasts will be inserted in the HTML code:
                // .... Set to BOOLEAN TRUE and the toast messages will be inserted before those already generated toasts.
                // .... Set to BOOLEAN FALSE otherwise.
                insertBefore: true,
                // INTEGER: duration that the toast will be displayed in milliseconds:
                // .... Default value is set to 4000 (4 seconds).
                // .... If it set to 0, the duration for each toast is calculated by text-message length.
                duration: 7000,
                // BOOLEAN: enable or disable toast sounds:
                // .... Set to BOOLEAN TRUE  - to enable toast sounds.
                // .... Set to BOOLEAN FALSE - otherwise.
                // NOTE: this is not supported by mobile devices.
                enableSounds: false,
                // BOOLEAN: enable or disable auto hiding on toast messages:
                // .... Set to BOOLEAN TRUE  - to enable auto hiding.
                // .... Set to BOOLEAN FALSE - disable auto hiding. Instead the user must click on toast message to close it.
                autoClose: true,
                // BOOLEAN: enable or disable the progressbar:
                // .... Set to BOOLEAN TRUE  - enable the progressbar only if the autoClose option value is set to BOOLEAN TRUE.
                // .... Set to BOOLEAN FALSE - disable the progressbar.
                progressBar: true,

                // the placement where prepend the toast container:
                prependTo: document.body.childNodes[0]
            };

            // put this right in your main.js file:
            var toast = new Toasty(options);

            if (toastStatus == "" || toastStatus == null) {
                toast.info(toastMessage);
            } else {
                switch (toastStatus) {
                    case "info":
                        toast.info(toastMessage);
                        break;
                    case "success":
                        toast.success(toastMessage);
                        break;
                    case "warning":
                        toast.warning(toastMessage);
                        break;
                    case "error":
                        toast.error(toastMessage);
                        break;
                    default:
                        toast.info(toastMessage);
                }
            }
        }
    });
}