

window.document.addEventListener("DOMContentLoaded", function () {
    const searchForm = document.getElementById("search-form");
    const agenticSearch = document.getElementById("agenticSearch");
    searchForm.addEventListener("submit", function (event) {

        var url = '/ai-search';
        event.preventDefault();

        if (agenticSearch.checked)
        {
            url = '/agentic-ai-search';
        };
    
        const input = document.getElementById("Input");
        const topK = document.getElementById("TopK");
        
        fetch(url, {
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




