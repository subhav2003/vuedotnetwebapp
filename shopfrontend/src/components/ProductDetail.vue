<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60 px-4">
    <Toast :toasts="toasts" @close="removeToast" />

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

        <!-- Reviews -->
        <div class="mt-10">
          <h3 class="text-xl font-semibold mb-2">Reviews</h3>
          <div v-if="reviews.length" class="space-y-3">
            <div
                v-for="(review, i) in reviews"
                :key="i"
                class="bg-[#2a2a2a] p-4 rounded shadow"
            >
              <div class="flex justify-between items-start mb-1">
                <div class="flex space-x-1">
                  <font-awesome-icon
                      v-for="s in 5"
                      :key="s"
                      :icon="s <= review.rating ? ['fas','star'] : ['far','star']"
                      class="w-4 h-4 text-yellow-400"
                  />
                </div>
                <div v-if="review.memberId === userId">
                  <template v-if="editingReviewId !== review.id">
                    <button class="text-sm text-blue-400 hover:underline mr-2" @click="startEdit(review)">Edit</button>
                    <button class="text-sm text-red-400 hover:underline" @click="deleteReview(review.id)">Delete</button>
                  </template>
                  <template v-else>
                    <button class="text-sm text-green-400 hover:underline mr-2" @click="submitEdit(review.id)">Submit</button>
                    <button class="text-sm text-gray-400 hover:underline" @click="cancelEdit">Cancel</button>
                  </template>
                </div>
              </div>
              <p class="font-semibold">{{ review.memberName }}</p>
              <p class="text-sm text-[#ccc]" v-if="editingReviewId !== review.id">{{ review.comment }}</p>
              <textarea
                  v-else
                  v-model="editedComment"
                  class="w-full p-2 mt-1 bg-[#1a1a1a] border border-[#444] text-[#eee] rounded"
              />
            </div>
          </div>
          <p v-else class="text-sm italic text-[#999]">No reviews yet.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import Toast from '@/components/Toast.vue';

const props = defineProps({ book: Object });
const emit = defineEmits(['close']);

const apiBaseUrl = process.env.VUE_APP_API_URL;
const bookData = ref({});
const selectedImage = ref(null);
const zoomedImage = ref(null);
const loading = ref(true);
const reviews = ref([]);
const userId = ref(Number(localStorage.getItem('userId')));
const editingReviewId = ref(null);
const editedComment = ref('');

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

const startEdit = (review) => {
  editingReviewId.value = review.id;
  editedComment.value = review.comment;
};

const cancelEdit = () => {
  editingReviewId.value = null;
  editedComment.value = '';
};

const submitEdit = async (id) => {
  try {
    const token = localStorage.getItem('authToken');
    await axios.put(`${apiBaseUrl}/api/review/${id}`, {
      rating: 5,
      comment: editedComment.value
    }, {
      headers: { Authorization: `Bearer ${token}` }
    });
    const index = reviews.value.findIndex(r => r.id === id);
    if (index !== -1) reviews.value[index].comment = editedComment.value;
    cancelEdit();
    showToast('Review updated successfully.');
  } catch (err) {
    showToast('Failed to update review.', 'error');
  }
};

const deleteReview = async (id) => {
  try {
    const token = localStorage.getItem('authToken');
    await axios.delete(`${apiBaseUrl}/api/review/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    reviews.value = reviews.value.filter(r => r.id !== id);
    showToast('Review deleted successfully.');
  } catch (err) {
    showToast('Failed to delete review.', 'error');
  }
};

const toasts = ref([]);
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.value.push({ id, message, type });
  setTimeout(() => removeToast(id), 4000);
}
function removeToast(id) {
  const i = toasts.value.findIndex(t => t.id === id);
  if (i !== -1) toasts.value.splice(i, 1);
}

onMounted(async () => {
  try {
    const bookRes = await axios.get(`${apiBaseUrl}/api/Book/${props.book.id}`);
    bookData.value = bookRes.data.data;
    selectedImage.value = apiBaseUrl + (bookData.value.images?.[0] || '');

    const reviewRes = await axios.get(`${apiBaseUrl}/api/review/book/${props.book.id}`);
    reviews.value = reviewRes.data;
  } catch (err) {
    console.error('Failed to load book or reviews:', err);
  } finally {
    loading.value = false;
  }
});
</script>