<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60 px-4">
    <div class="relative w-full max-w-4xl bg-[#1a1a1a] text-[#C8B280] rounded-lg shadow-xl overflow-y-auto max-h-[95vh]">
      <!-- Close Button -->
      <button @click="close" class="absolute top-4 right-4 text-[#aaa] hover:text-red-600 transition">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>

      <div class="p-6 pt-10">
        <div v-if="loading" class="flex justify-center items-center h-64">
          <svg class="animate-spin h-10 w-10 text-[#C8B280]" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
          </svg>
        </div>

        <div v-else class="flex flex-col lg:flex-row gap-8">
          <!-- Left: Image Gallery -->
          <div class="lg:w-1/2 flex flex-col items-center">
            <img
                :src="selectedImage"
                class="rounded w-full max-h-80 object-contain mb-4 cursor-zoom-in"
                @click="zoomImage(selectedImage)"
            />
            <div class="flex space-x-2 overflow-x-auto">
              <img
                  v-for="(img, i) in bookData.images"
                  :key="i"
                  :src="apiBaseUrl + img"
                  class="w-16 h-16 object-cover rounded cursor-pointer hover:ring-2 hover:ring-[#A0522D]"
                  :class="{ 'ring-2 ring-[#A0522D]': selectedImage === (apiBaseUrl + img) }"
                  @click="selectedImage = apiBaseUrl + img"
              />
            </div>
          </div>

          <!-- Right: Info -->
          <div class="lg:w-1/2 space-y-3">
            <h2 class="text-3xl font-bold">{{ bookData.title }}</h2>
            <p class="text-sm text-[#d1c2a0]">by {{ bookData.author }}</p>
            <p class="text-2xl font-semibold mt-2">$ {{ bookData.price }}</p>

            <div class="mt-4 grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm">
              <p><strong>Genre:</strong> {{ bookData.genreName }}</p>
              <p><strong>Stock:</strong> {{ bookData.stock }}</p>
              <p><strong>Format:</strong> {{ bookData.format }}</p>
              <p><strong>Language:</strong> {{ bookData.language }}</p>
              <p><strong>ISBN:</strong> {{ bookData.isbn || 'N/A' }}</p>
              <p><strong>Book Type:</strong> {{ bookData.bookType || 'N/A' }}</p>
              <p><strong>Publisher:</strong> {{ bookData.publisher || 'N/A' }}</p>
              <p><strong>Exclusive Edition:</strong> {{ bookData.isExclusiveEdition ? 'Yes' : 'No' }}</p>
              <p><strong>Digital Access:</strong> {{ bookData.isPhysicalAccess ? 'No' : 'Yes' }}</p>
              <p><strong>Physical Copy:</strong> {{ bookData.isPhysicalAccess ? 'Yes' : 'No' }}</p>
              <p><strong>Published:</strong> {{ formatDate(bookData.publicationDate) }}</p>
              <p v-if="bookData.isOnSale"><strong>Discount:</strong> {{ bookData.discountPercentage }}%</p>
              <p v-if="bookData.isOnSale"><strong>Discount Period:</strong> {{ formatDate(bookData.discountStart) }} - {{ formatDate(bookData.discountEnd) }}</p>
            </div>

            <div class="mt-4">
              <p class="text-sm leading-relaxed"><strong>Description:</strong> {{ bookData.description || 'No description.' }}</p>
            </div>

            <div class="mt-4 flex space-x-4">
              <slot name="flag" />
              <slot name="add-to-cart" />
            </div>
          </div>
        </div>

        <!-- Reviews (Placeholder) -->
        <div class="mt-10">
          <h3 class="text-xl font-semibold mb-2">Reviews</h3>
          <div class="space-y-3">
            <div v-for="(review, i) in dummyReviews" :key="i" class="bg-[#2a2a2a] p-4 rounded shadow">
              <div class="flex space-x-1 mb-1">
                <font-awesome-icon
                    v-for="s in 5"
                    :key="s"
                    :icon="s <= review.rating ? ['fas','star'] : ['far','star']"
                    class="w-4 h-4 text-yellow-400"
                />
              </div>
              <p class="font-semibold">{{ review.username }}</p>
              <p class="text-sm text-[#ccc]">{{ review.review }}</p>
            </div>
          </div>
          <p class="text-xs text-gray-500 mt-3 italic">Note: These are dummy reviews for now.</p>
        </div>
      </div>
    </div>

    <!-- Zoomed Image Modal -->
    <div v-if="zoomedImage" class="fixed inset-0 bg-black bg-opacity-90 z-50 flex justify-center items-center">
      <div class="relative">
        <img :src="zoomedImage" class="max-h-[90vh] rounded-lg border border-white shadow-2xl" />
        <button @click="zoomedImage = null" class="absolute top-4 right-4 text-white text-2xl hover:text-red-500">×</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const props = defineProps({
  book: { type: Object, required: true }
});
const emit = defineEmits(['close']);

const apiBaseUrl = process.env.VUE_APP_API_URL;
const bookData = ref({});
const selectedImage = ref(null);
const zoomedImage = ref(null);
const loading = ref(true);

const dummyReviews = [
  { username: 'Alice', rating: 5, review: 'An excellent read. Highly recommended!' },
  { username: 'Bob', rating: 4, review: 'Well-written and thought-provoking.' },
  { username: 'Charlie', rating: 3, review: 'Good, but could’ve been shorter.' }
];

function close() {
  emit('close');
}

function zoomImage(img) {
  zoomedImage.value = img;
}

function formatDate(dateStr) {
  if (!dateStr) return 'N/A';
  const date = new Date(dateStr);
  return date.toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
}

onMounted(async () => {
  try {
    const res = await axios.get(`${apiBaseUrl}/api/Book/${props.book.id}`);
    bookData.value = res.data.data;
    selectedImage.value = apiBaseUrl + (bookData.value.images?.[0] || '');
  } catch (err) {
    console.error('Failed to load book:', err);
  } finally {
    loading.value = false;
  }
});
</script>