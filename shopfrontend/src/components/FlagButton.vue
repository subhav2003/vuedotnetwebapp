<template>
  <div class="flex flex-col items-start gap-1">
    <button
        v-if="isAuthenticated"
        class="divide-white hover:text-red-800 text-sm flex items-center gap-1"
        :disabled="isFlagged || loading"
        @click="sendFlag"
    >
      <span v-if="!isFlagged && !loading">🏳️ Bookmark</span>
      <span v-else-if="loading">Flagging...</span>
      <span v-else class="text-green-600">Flagged ✓</span>
    </button>

    <!-- Show error message if any -->
    <span v-if="errorMessage" class="text-xs text-red-500">{{ errorMessage }}</span>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import axios from 'axios'

const props = defineProps({
  type: {
    type: String,
    required: true, // 'artisan', 'product', or 'review'
  },
  id: {
    type: Number,
    required: true,
  }
})

const emit = defineEmits(['flagged'])

const isFlagged = ref(false)
const loading = ref(false)
const errorMessage = ref('')

const isAuthenticated = computed(() => !!localStorage.getItem('authToken'))

const apiBaseUrl = process.env.VUE_APP_API_URL

const sendFlag = async () => {
  loading.value = true
  errorMessage.value = ''
  try {
    await axios.post(`${apiBaseUrl}/api/v1/moderation/flag`, {
      type: props.type,
      id: props.id
    }, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken')}`
      }
    })

    isFlagged.value = true
    emit('flagged', { type: props.type, id: props.id })
  } catch (err) {
    console.error('Flagging failed:', err)
    errorMessage.value = err.response?.data?.message || 'Failed to flag item. Please try again later.'
  } finally {
    loading.value = false
  }
}
</script>
