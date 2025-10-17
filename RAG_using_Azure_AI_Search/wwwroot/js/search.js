

window.document.addEventListener("DOMContentLoaded", function () {
    const searchForm = document.getElementById("search-form");

    searchForm.addEventListener("submit", function (event) {

        event.preventDefault();

        const input = document.getElementById("Input");
        const topK = document.getElementById("TopK");
        
        fetch("/ai-search", {
            method: "POST",
            body: JSON.stringify({
                input : input.value,
                topK : topK.value
            }),
            headers: {
                "Content-Type": "application/json"
            }
        }).then(response => response.json())
            .then(data => {
                const searchResult = document.getElementById("searchResult");
                searchResult.innerHTML = data.response;
            })
    });

}); 




