document.addEventListener("DOMContentLoaded", function () {
  function checkImage(imgElement, messageElement) {
    imgElement.onerror = function () {
      imgElement.classList.add("d-none");
      messageElement.classList.remove("d-none");
    };
    if (!imgElement.src || imgElement.src === "path/to/eye_photo1.jpg") {
      imgElement.onerror();
    }
  }

  var eyePhoto1 = document.getElementById("eye_photo1");
  var noPhotoMessage1 = document.getElementById("no_photo_message1");
  checkImage(eyePhoto1, noPhotoMessage1);
});
