<template>
  <!-- Toasts Stack -->
  <Toast :toasts="toasts" @close="removeToast" />

  <!-- Container for the review form -->
  <div class="relative justify-center content-center p-6 bg-white rounded-lg shadow-lg w-[90%] sm:w-[500px]">
    <!-- Title -->
    <h2 class="text-xl font-semibold mb-4">
      Write a Review for {{ product?.name || 'Product' }}
    </h2>

    <!-- Inline Rating Stars (with hover highlight) -->
    <div class="flex items-center space-x-2 mb-4">
      <font-awesome-icon
          v-for="star in 5"
          :key="star"
          :icon="displayedRating >= star ? ['fas','star'] : ['far','star']"
          class="w-6 h-6 cursor-pointer text-yellow-500"
          @mouseover="handleStarHover(star)"
          @mouseleave="handleStarLeave"
          @click="handleStarClick(star)"
      />
    </div>

    <!-- Review Text Area -->
    <textarea
        v-model="review"
        placeholder="Write your review here..."
        class="w-full p-2 border rounded-md text-sm"
    ></textarea>

    <!-- Action Buttons -->
    <div class="mt-4 flex items-center space-x-2">
      <!-- Submit Review Button -->
      <button
          class="bg-green-500 text-white px-4 py-2 rounded-md hover:bg-green-600 transition-colors"
          @click="submitReview"
      >
        Submit Review
      </button>

      <!-- Close Button -->
      <button
          class="bg-gray-300 text-black px-4 py-2 rounded-md hover:bg-gray-400 transition-colors"
          @click="closeForm"
      >
        Close
      </button>
    </div>

    <!-- "X" Icon Close Button (top-right) -->
    <button
        @click="closeForm"
        class="absolute top-2 right-2 text-gray-500 hover:text-gray-700 p-2"
    >
      <svg
          xmlns="http://www.w3.org/2000/svg"
          class="w-6 h-6"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
      >
        <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M6 18L18 6M6 6l12 12"
        />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, defineProps, defineEmits, computed, reactive } from 'vue';
import Toast from '@/components/Toast.vue';

// Props & Emits
const props = defineProps({ product: Object });
const emit = defineEmits(['close']);

// Toast stack
const toasts = reactive([]);
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 3000);
}

// Review form state
const rating = ref(0);
const hoveredRating = ref(0);
const review = ref('');

const displayedRating = computed(() =>
    hoveredRating.value > 0 ? hoveredRating.value : rating.value
);

// Star handlers
function handleStarHover(star) { hoveredRating.value = star; }
function handleStarLeave() { hoveredRating.value = 0; }
function handleStarClick(star) { rating.value = star; }

// Submit Review
async function submitReview() {
  if (!props.product) return;
  const token = localStorage.getItem('authToken');
  try {
    const res = await fetch(
        `${process.env.VUE_APP_API_URL}/api/v1/reviews/store`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            product_id: props.product.id,
            rating: rating.value,
            review: review.value,
          }),
        }
    );

    const data = await res.json();
    if (!res.ok) {
      showToast(data.message || 'Failed to submit review.', 'error');
      return;
    }

    showToast(data.message || 'Review submitted successfully!', 'success');
    // reset
    rating.value = 0;
    hoveredRating.value = 0;
    review.value = '';
  } catch (err) {
    console.error(err);
    showToast('Network error submitting review.', 'error');
  }
}

// Close form
function closeForm() {
  emit('close');
}
</script>

<style scoped>
/* No additional styles */
</style>
